using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using test_2.Models;
using test_2.Services;

namespace finalProject_1607.Controllers
{
    [Route("payment")]
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPaymentService _paymentService;
        private readonly MyGarageFinalContext _context;
        private readonly PayOSService _payOSService;

        public PaymentController(IHttpClientFactory httpClientFactory, IPaymentService paymentService, MyGarageFinalContext context, PayOSService payOSService)
        {
            _httpClientFactory = httpClientFactory;
            _paymentService = paymentService;
            _context = context;
            _payOSService = payOSService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment(decimal amount, string description, int? appointmentId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                Console.WriteLine($"UserID: {userId}");

                if (userId == null)
                    return BadRequest("Vui lòng đăng nhập để thanh toán.");

                if (!appointmentId.HasValue)
                    return BadRequest("Thiếu thông tin lịch hẹn.");

                var appointment = await _context.Appointments
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId.Value);

                if (appointment == null)
                    return BadRequest("Không tìm thấy thông tin lịch hẹn.");

                // Chỉ cho phép thanh toán khi lịch hẹn đã hoàn thành
                if (appointment.Status != "Completed")
                    return BadRequest("Chỉ có thể thanh toán sau khi kỹ thuật viên đã hoàn thành sửa chữa.");

                var payment = await _paymentService.CreatePaymentAsync(appointmentId.Value, userId.Value, amount, description);

                // orderCode: use only payment.PaymentId (long, positive, < 9007199254740991)
                long orderCode = payment.PaymentId;

                // description: max 25 chars
                var safeDescription = description.Length > 25 ? description.Substring(0, 25) : description;

                // Sử dụng PayOS Service để tạo payment request
                var paymentRequest = new PayOSPaymentRequest
                {
                    OrderCode = orderCode, // long, not string
                    Amount = (long)amount,
                    Description = safeDescription,
                    CancelUrl = $"{Request.Scheme}://{Request.Host}/payment/cancel?paymentId={payment.PaymentId}",
                    ReturnUrl = $"{Request.Scheme}://{Request.Host}/payment/success?paymentId={payment.PaymentId}",
                    BuyerName = appointment.User?.FullName ?? "Khách hàng",
                    BuyerEmail = appointment.User?.Email ?? "",
                    BuyerPhone = appointment.User?.Phone ?? "",
                    BuyerAddress = "",
                    Items = new List<PayOSPaymentItem>
                    {
                        new PayOSPaymentItem
                        {
                            Name = "Dịch vụ sửa chữa xe",
                            Quantity = 1,
                            Price = (long)amount
                        }
                    }
                };
                Console.WriteLine($"paymentRequest: {paymentRequest}");
                Console.WriteLine($"🚀 Creating payment with PayOS Service...");
                Console.WriteLine($"📋 Order Code: {orderCode}");
                Console.WriteLine($"💰 Amount: {amount:N0} VND");

                try
                {
                    // Gọi PayOS Service
                    var result = await _payOSService.CreatePaymentRequest(paymentRequest);
                    
                    Console.WriteLine($"✅ PayOS Service Response: {JsonSerializer.Serialize(result)}");

                    if (result != null && !string.IsNullOrEmpty(result.CheckoutUrl))
                    {
                        // Lưu order code vào database
                        payment.PayOSOrderCode = orderCode.ToString(); // Save as string for DB
                        await _context.SaveChangesAsync();
                        
                        Console.WriteLine($"🔗 Redirecting to: {result.CheckoutUrl}");
                        return Redirect(result.CheckoutUrl);
                    }
                    else
                    {
                        Console.WriteLine("❌ PayOS Service returned null or empty checkout URL");
                        throw new Exception("Không thể tạo link thanh toán từ PayOS");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ PayOS Service Error: {ex.Message}");
                    Console.WriteLine("🔄 Redirecting to Mock Payment Gateway...");
                    
                    // Fallback to Mock Gateway
                    return RedirectToAction("MockCreatePayment", new { 
                amount = amount,
                description = description,
                        appointmentId = appointmentId 
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ General Error: {ex.Message}");
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }

        [HttpGet("create")]
        [AllowAnonymous]
        public IActionResult Create(decimal amount, string description, int? appointmentId = null)
        {
            ViewBag.Amount = amount;
            ViewBag.Description = description;
            ViewBag.AppointmentId = appointmentId;
            return View("~/Views/Payment/Test.cshtml");
        }

        [HttpGet("ConfirmExtraPayment")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmExtraPayment(decimal amount, string description, int appointmentId)
        {
            MultiVehicleAppointmentViewModel? model = null;
            if (TempData["EditMultiModel"] != null)
            {
                try
                {
                    model = System.Text.Json.JsonSerializer.Deserialize<MultiVehicleAppointmentViewModel>(TempData["EditMultiModel"].ToString());
                    if (model != null)
                    {
                        var userIdStr = HttpContext.Session.GetString("UserId");
                        int? userId = null;
                        if (int.TryParse(userIdStr, out int uid)) userId = uid;
                        await DropdownHelper.LoadDropdownsForMulti(_context, model, userId);
                    }
                }
                catch { }
            }
            ViewBag.Amount = amount;
            ViewBag.Description = description;
            ViewBag.AppointmentId = appointmentId;
            ViewBag.OldTotal = TempData["EditMultiOldTotal"];
            ViewBag.NewTotal = TempData["EditMultiNewTotal"];
            ViewBag.Diff = TempData["EditMultiDiff"];
            return View("~/Views/Payment/ConfirmExtraPayment.cshtml", model);
        }

        private int? GetCurrentUserId()
        {
            // Thử lấy từ session (cách chính xác nhất)
            var sessionUserId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(sessionUserId) && int.TryParse(sessionUserId, out int sessionId))
            {
                Console.WriteLine($"✅ Found UserId in session: {sessionId}");
                return sessionId;
            }
            
            // Thử lấy từ claim UserID
            var userIdClaim = User.FindFirst("UserID");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                Console.WriteLine($"✅ Found UserId in claim: {userId}");
                return userId;
            }
            
            // Thử lấy từ claim NameIdentifier
            var nameIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (nameIdClaim != null && int.TryParse(nameIdClaim.Value, out int nameId))
            {
                Console.WriteLine($"✅ Found UserId in NameIdentifier: {nameId}");
                return nameId;
            }
            
            Console.WriteLine("❌ Không tìm thấy UserID trong claims hoặc session");
            Console.WriteLine($"Session keys: {string.Join(", ", HttpContext.Session.Keys)}");
            return null;
        }

        [HttpPost("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentCallback([FromBody] object data)
        {
            try
            {
                // TODO: Xác thực checksum từ PayOS
                Console.WriteLine($"PayOS Callback: {System.Text.Json.JsonSerializer.Serialize(data)}");
                
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(System.Text.Json.JsonSerializer.Serialize(data));
                
                if (jsonElement.TryGetProperty("data", out var dataElement))
                {
                    string? orderCode = null;
                    string? status = null;
                    string? transactionId = null;
                    
                    if (dataElement.TryGetProperty("orderCode", out var orderCodeElement))
                        orderCode = orderCodeElement.GetString();
                    
                    if (dataElement.TryGetProperty("status", out var statusElement))
                        status = statusElement.GetString();
                    
                    if (dataElement.TryGetProperty("transactionId", out var transactionIdElement))
                        transactionId = transactionIdElement.GetString();
                    
                    if (!string.IsNullOrEmpty(orderCode))
                    {
                        var payment = await _paymentService.UpdatePaymentStatusAsync(orderCode, status ?? "Unknown", transactionId);
                        
                        // ✅ Nếu thanh toán thành công, chỉ gửi email xác nhận (không thay đổi appointment status)
                        if (status == "PAID" && payment != null)
                        {
                            // ✅ KHÔNG thay đổi appointment status - giữ nguyên "Pending"
                            Console.WriteLine($"✅ Payment {payment.PaymentId} successful via callback - Appointment {payment.Appointment?.AppointmentId} status remains Pending");
                            
                            // Gửi email xác nhận
                            if (payment.User?.Email != null)
                            {
                                await _paymentService.SendPaymentConfirmationEmailAsync(payment, payment.User.Email);
                            }
                        }
                    }
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PayOS Callback Error: {ex.Message}");
                return BadRequest();
            }
        }

        [HttpGet("success")]
        [AllowAnonymous]
        public async Task<IActionResult> Success(int paymentId)
        {
            var payment = await _context.PaymentHistories
                .Include(p => p.Appointment)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                return NotFound("Không tìm thấy thông tin thanh toán.");
            }

            // Cập nhật trạng thái payment thành công
            payment.Status = "Success";
            payment.UpdatedAt = DateTime.Now;
            
            // ✅ KHÔNG thay đổi trạng thái lịch hẹn - giữ nguyên "Pending"
            Console.WriteLine($"✅ Payment {payment.PaymentId} successful - Appointment {payment.Appointment?.AppointmentId} status remains Pending");
            
            await _context.SaveChangesAsync();

            ViewBag.Payment = payment;
            return View("~/Views/Payment/Success.cshtml");
        }

        [HttpGet("cancel")]
        [AllowAnonymous]
        public async Task<IActionResult> Cancel(int paymentId)
        {
            var payment = await _context.PaymentHistories
                .Include(p => p.Appointment)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                return NotFound("Không tìm thấy thông tin thanh toán.");
            }

            // ❌ Chỉ cập nhật trạng thái payment thành Cancelled, KHÔNG thay đổi trạng thái lịch hẹn
            payment.Status = "Cancelled";
            payment.UpdatedAt = DateTime.Now;
            
            Console.WriteLine($"❌ Payment {payment.PaymentId} cancelled - Appointment status unchanged");
            
            await _context.SaveChangesAsync();

            ViewBag.Payment = payment;
            return View("~/Views/Payment/Cancel.cshtml");
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                
                Console.WriteLine($"📥 Webhook received: {body}");
                
                // Sử dụng PayOS Service để verify webhook
                var signature = Request.Headers["x-signature"].ToString();
                var isValid = _payOSService.VerifyWebhookData(body, signature);
                
                if (!isValid)
                {
                    Console.WriteLine("❌ Invalid webhook signature");
                    return BadRequest("Invalid signature");
                }
                
                // Parse webhook data
                var webhookData = JsonSerializer.Deserialize<JsonElement>(body);
                
                if (webhookData.TryGetProperty("data", out var data) && 
                    data.TryGetProperty("orderCode", out var orderCode))
                {
                    var orderCodeStr = orderCode.GetString();
                    var payment = await _context.PaymentHistories
                        .Include(p => p.User)
                        .FirstOrDefaultAsync(p => p.PayOSOrderCode == orderCodeStr);
                    
                    if (payment != null)
                    {
                        // Cập nhật trạng thái thanh toán
                        payment.Status = "Success";
                        payment.UpdatedAt = DateTime.Now;
                        
                        // ✅ KHÔNG thay đổi trạng thái lịch hẹn - giữ nguyên "Pending"
                        Console.WriteLine($"✅ Payment {payment.PaymentId} updated to Success via webhook - Appointment {payment.Appointment?.AppointmentId} status remains Pending");
                        
                        await _context.SaveChangesAsync();
                        
                        // Gửi email xác nhận
                        if (payment.User?.Email != null)
                        {
                            await _paymentService.SendPaymentConfirmationEmailAsync(payment, payment.User.Email);
                        }
                        
                        Console.WriteLine($"✅ Payment {payment.PaymentId} updated to Success");
                    }
                }
                
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Webhook error: {ex.Message}");
                return BadRequest();
            }
        }

        [HttpGet("test-page")]
        [AllowAnonymous]
        public IActionResult TestPage()
        {
            return View("~/Views/Payment/Test.cshtml");
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public async Task<IActionResult> TestPayOS()
        {
            try
            {
                var orderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // long
                var amount = 1000L;
                var description = "Test payment";

                // Sử dụng PayOS Service để test
                var paymentRequest = new PayOSPaymentRequest
                {
                    OrderCode = orderCode,
                    Amount = amount,
                    Description = description,
                    CancelUrl = $"{Request.Scheme}://{Request.Host}/payment/cancel",
                    ReturnUrl = $"{Request.Scheme}://{Request.Host}/payment/success",
                    BuyerName = "Test User",
                    BuyerEmail = "test@example.com",
                    BuyerPhone = "0123456789",
                    BuyerAddress = "",
                    Items = new List<PayOSPaymentItem>
                    {
                        new PayOSPaymentItem
                        {
                            Name = "Test service",
                            Quantity = 1,
                            Price = amount
                        }
                    }
                };

                Console.WriteLine($"🚀 Testing PayOS Service...");
                Console.WriteLine($"📋 Order Code: {orderCode}");
                Console.WriteLine($"💰 Amount: {amount:N0} VND");

                try
                {
                    // Gọi PayOS Service
                    var result = await _payOSService.CreatePaymentRequest(paymentRequest);
                    
                    Console.WriteLine($"✅ PayOS Service Test Response: {JsonSerializer.Serialize(result)}");

                    if (result != null && !string.IsNullOrEmpty(result.CheckoutUrl))
                    {
                        return Content($@"
                            <div style='font-family: Arial, sans-serif; margin: 20px;'>
                                <h2>✅ PayOS Service Test Success!</h2>
                                <p><strong>Order Code:</strong> {orderCode}</p>
                                <p><strong>Amount:</strong> {amount:N0} VND</p>
                                <p><strong>Checkout URL:</strong> <a href='{result.CheckoutUrl}' target='_blank'>{result.CheckoutUrl}</a></p>
                                <p><strong>Response:</strong> {JsonSerializer.Serialize(result)}</p>
                                <br>
                                <a href='{result.CheckoutUrl}' target='_blank' style='background: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>🔗 Go to Payment Page</a>
                                <br><br>
                                <a href='/payment/mock-create?amount=100000&description=Test Payment' style='background: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>🔧 Test Mock Gateway</a>
                            </div>", "text/html");
                    }
                    else
                    {
                        throw new Exception("PayOS Service returned null or empty checkout URL");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ PayOS Service Test Error: {ex.Message}");
                    return Content($@"
                        <div style='font-family: Arial, sans-serif; margin: 20px;'>
                            <h2>❌ PayOS Service Test Failed</h2>
                            <p><strong>Error:</strong> {ex.Message}</p>
                            <br>
                            <h3>🔧 Mock Payment Gateway Available</h3>
                            <p>You can test the payment flow using our mock gateway:</p>
                            <a href='/payment/mock-create?amount=100000&description=Test Payment' style='background: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>🔧 Test Mock Gateway</a>
                        </div>", "text/html");
                }
            }
            catch (Exception ex)
            {
                return Content($"❌ Test failed: {ex.Message}", "text/html");
            }
        }

        [HttpGet("mock-create")]
        [AllowAnonymous]
        public async Task<IActionResult> MockCreatePayment(decimal amount, string description, int? appointmentId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return RedirectToAction("Login", "AccountLogin");
                }

                var user = await _context.Users.FindAsync(userId.Value);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Tạo payment record
                var payment = new PaymentHistory
                {
                    UserId = userId.Value,
                    AppointmentId = appointmentId,
                    Amount = amount,
                    Description = description,
                    Status = "Pending",
                    PayOSOrderCode = $"MOCK_ORDER_{DateTime.Now:yyyyMMddHHmmss}",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.PaymentHistories.Add(payment);
                await _context.SaveChangesAsync();

                // Tạo mock payment URL
                var mockPaymentUrl = $"{Request.Scheme}://{Request.Host}/payment/mock-process?paymentId={payment.PaymentId}";

                return Content($@"
                    <html>
                    <head>
                        <title>Mock Payment Gateway</title>
                        <style>
                            body {{ font-family: Arial, sans-serif; margin: 40px; }}
                            .container {{ max-width: 600px; margin: 0 auto; }}
                            .header {{ background: #007bff; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                            .content {{ background: #f8f9fa; padding: 20px; border-radius: 0 0 5px 5px; }}
                            .btn {{ display: inline-block; padding: 10px 20px; background: #28a745; color: white; text-decoration: none; border-radius: 5px; margin: 5px; }}
                            .btn-danger {{ background: #dc3545; }}
                            .info {{ background: #e7f3ff; padding: 10px; border-radius: 5px; margin: 10px 0; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>🔧 Mock Payment Gateway</h2>
                                <p>Testing Payment Flow</p>
                            </div>
                            <div class='content'>
                                <div class='info'>
                                    <strong>Payment Details:</strong><br>
                                    Amount: {amount:N0} VND<br>
                                    Description: {description}<br>
                                    Payment ID: {payment.PaymentId}
                                </div>
                                
                                <p>This is a mock payment gateway for testing purposes. In production, this would redirect to PayOS.</p>
                                
                                <a href='/payment/mock-success?paymentId={payment.PaymentId}' class='btn'>✅ Simulate Success</a>
                                <a href='/payment/mock-cancel?paymentId={payment.PaymentId}' class='btn btn-danger'>❌ Simulate Cancel</a>
                                <a href='/payment/cancel?paymentId={payment.PaymentId}' class='btn btn-danger'>🔙 Back to Cancel</a>
                            </div>
                        </div>
                    </body>
                    </html>", "text/html");
            }
            catch (Exception ex)
            {
                return Content($"❌ Mock payment creation failed: {ex.Message}", "text/html");
            }
        }

        [HttpGet("mock-success")]
        [AllowAnonymous]
        public async Task<IActionResult> MockSuccess(int paymentId)
        {
            try
            {
                var payment = await _context.PaymentHistories
                    .Include(p => p.Appointment)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                if (payment == null)
                {
                    return NotFound("Payment not found");
                }

                // Cập nhật trạng thái thành công
                payment.Status = "Success";
                payment.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                // Gửi email xác nhận
                if (payment.User?.Email != null)
                {
                    await _paymentService.SendPaymentConfirmationEmailAsync(payment, payment.User.Email);
                }

                return RedirectToAction("Success", new { paymentId = paymentId });
            }
            catch (Exception ex)
            {
                return Content($"❌ Mock success failed: {ex.Message}", "text/html");
            }
        }

        [HttpGet("mock-cancel")]
        [AllowAnonymous]
        public async Task<IActionResult> MockCancel(int paymentId)
        {
            try
            {
                var payment = await _context.PaymentHistories
                    .Include(p => p.Appointment)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                if (payment == null)
                {
                    return NotFound("Payment not found");
                }

                // Cập nhật trạng thái hủy
                payment.Status = "Cancelled";
                payment.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return RedirectToAction("Cancel", new { paymentId = paymentId });
            }
            catch (Exception ex)
            {
                return Content($"❌ Mock cancel failed: {ex.Message}", "text/html");
            }
        }
    }
}