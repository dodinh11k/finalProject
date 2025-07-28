using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test_2.Models;
using test_2.Services;

namespace finalProject_1607.Controllers
{
    [Route("invoice")]
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly MyGarageFinalContext _context;
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(MyGarageFinalContext context, IInvoiceService invoiceService)
        {
            _context = context;
            _invoiceService = invoiceService;
        }

        [HttpGet("appointment/{id}")]
        public async Task<IActionResult> DownloadAppointmentInvoice(int id)
        {
            try
            {
                Console.WriteLine($"=== BẮT ĐẦU TẠO PDF CHO APPOINTMENT {id} ===");
                
                var userId = GetCurrentUserId();
                Console.WriteLine($"User ID: {userId}");
                
                if (!userId.HasValue)
                {
                    return RedirectToAction("Login", "AccountLogin");
                }

                // Lấy thông tin appointment đầy đủ
                var appointment = await _context.Appointments
                    .Include(a => a.User)
                    .Include(a => a.Garage)
                    .FirstOrDefaultAsync(a => a.AppointmentId == id && a.UserId == userId.Value);

                Console.WriteLine($"Appointment found: {appointment != null}");
                Console.WriteLine($"Appointment ID: {appointment?.AppointmentId}");
                Console.WriteLine($"User: {appointment?.User?.FullName}");

                if (appointment == null)
                {
                    return NotFound("Không tìm thấy lịch hẹn");
                }

                // Lấy chi tiết dịch vụ
                var vehicleDetails = await _context.AppointmentVehicleDetails
                    .Include(d => d.Vehicle)
                    .Include(d => d.Service)
                    .Include(d => d.Technician)
                    .Where(d => d.AppointmentId == id)
                    .ToListAsync();

                Console.WriteLine($"VehicleDetails count: {vehicleDetails?.Count ?? 0}");

                // Tính tổng tiền dịch vụ
                decimal totalServicePrice = vehicleDetails.Sum(d => ((d.Service?.Price ?? 0m) * (d.Quantity ?? 0)));

                // Lấy sản phẩm thay thế
                var replacementProducts = await _context.AppointmentProductDetails
                    .Include(p => p.Product)
                    .Where(p => p.AppointmentId == id)
                    .ToListAsync();

                Console.WriteLine($"ReplacementProducts count: {replacementProducts?.Count ?? 0}");

                decimal totalProductPrice = replacementProducts.Sum(p => p.UnitPrice * p.Quantity);
                decimal discountAmount = appointment.DiscountAmount ?? 0m;
                decimal totalAmount = appointment.TotalAmount ?? (totalServicePrice + totalProductPrice - discountAmount);

                // Chi tiết giá từng dịch vụ
                var servicePriceDetails = vehicleDetails
                    .Where(d => d.Service != null)
                    .GroupBy(d => d.Service.ServiceName)
                    .Select(g => (ServiceName: g.Key, Price: g.Sum(d => (d.Service.Price ?? 0m) * (d.Quantity ?? 0))))
                    .ToList();

                // Chi tiết các loại giảm giá
                var discountDetails = new List<(string DiscountType, decimal Amount)>();
                decimal loyaltyDiscount = 0m, carCountDiscount = 0m, promoDiscount = 0m;
                int previousAppointments = await _context.Appointments.CountAsync(a => a.UserId == appointment.UserId && a.AppointmentId != appointment.AppointmentId);
                if (previousAppointments >= 1)
                {
                    loyaltyDiscount = servicePriceDetails.Sum(x => x.Price) * 0.05m;
                    if (loyaltyDiscount > 0) discountDetails.Add(("Khách hàng thân thiết", loyaltyDiscount));
                }
                int carCount = vehicleDetails.Select(d => d.VehicleId).Distinct().Count();
                if (carCount >= 2)
                {
                    carCountDiscount = servicePriceDetails.Sum(x => x.Price) * 0.10m;
                    if (carCountDiscount > 0) discountDetails.Add(("Số lượng xe", carCountDiscount));
                }
                if (!string.IsNullOrEmpty(appointment.PromoCode))
                {
                    if (appointment.PromoCode.ToUpper() == "WELCOME10")
                    {
                        promoDiscount = servicePriceDetails.Sum(x => x.Price) * 0.10m;
                    }
                    else if (appointment.PromoCode.ToUpper() == "SUMMER2025")
                    {
                        promoDiscount = 50000m;
                    }
                    else
                    {
                        var voucher = await _context.PromoCodes.FirstOrDefaultAsync(p => p.Code.ToUpper() == appointment.PromoCode.ToUpper());
                        if (voucher != null)
                        {
                            if (voucher.DiscountPercent.HasValue)
                                promoDiscount = servicePriceDetails.Sum(x => x.Price) * (decimal)voucher.DiscountPercent.Value / 100;
                            else if (voucher.DiscountAmount.HasValue)
                                promoDiscount = voucher.DiscountAmount.Value;
                        }
                    }
                    if (promoDiscount > 0) discountDetails.Add(("Mã khuyến mãi", promoDiscount));
                }

                var viewModel = new AppointmentDetailsViewModel
                {
                    Appointment = appointment,
                    VehicleDetails = vehicleDetails,
                    TotalServicePrice = totalServicePrice,
                    DiscountAmount = discountAmount,
                    TotalAmount = totalAmount,
                    ServicePriceDetails = servicePriceDetails,
                    DiscountDetails = discountDetails,
                    ReplacementProducts = replacementProducts
                };

                Console.WriteLine($"ViewModel created successfully");
                Console.WriteLine($"TotalAmount: {viewModel.TotalAmount}");

                // Tạo PDF
                Console.WriteLine($"Calling GenerateInvoicePdf...");
                byte[] pdfBytes = _invoiceService.GenerateInvoicePdf(viewModel);
                Console.WriteLine($"PDF generated, length: {pdfBytes?.Length ?? 0}");

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return BadRequest("Không thể tạo PDF - dữ liệu rỗng");
                }

                // Trả về file PDF
                string fileName = $"HoaDon_LichHen_{appointment.AppointmentId}_{DateTime.Now:yyyyMMdd}.pdf";
                Console.WriteLine($"Returning file: {fileName}");
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in DownloadAppointmentInvoice: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"Lỗi tạo hóa đơn: {ex.Message}");
            }
        }

        [HttpGet("payment/{paymentId}")]
        public async Task<IActionResult> DownloadPaymentReceipt(int paymentId)
        {
            try
            {
                Console.WriteLine($"=== BẮT ĐẦU TẠO PDF CHO PAYMENT {paymentId} ===");
                
                var userId = GetCurrentUserId();
                Console.WriteLine($"User ID: {userId}");
                
                if (!userId.HasValue)
                {
                    return RedirectToAction("Login", "AccountLogin");
                }

                // Lấy thông tin payment đơn giản
                var payment = await _context.PaymentHistories
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId && p.UserId == userId.Value);

                Console.WriteLine($"Payment found: {payment != null}");
                Console.WriteLine($"Payment ID: {payment?.PaymentId}");
                Console.WriteLine($"Amount: {payment?.Amount}");

                if (payment == null)
                {
                    return NotFound("Không tìm thấy thông tin thanh toán");
                }

                // Lấy thông tin appointment đơn giản
                var appointment = await _context.Appointments
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.AppointmentId == payment.AppointmentId);

                Console.WriteLine($"Appointment found: {appointment != null}");
                Console.WriteLine($"Appointment ID: {appointment?.AppointmentId}");
                Console.WriteLine($"User: {appointment?.User?.FullName}");

                if (appointment == null)
                {
                    return NotFound("Không tìm thấy thông tin lịch hẹn");
                }

                // Tạo PDF
                Console.WriteLine($"Calling GeneratePaymentReceiptPdf...");
                byte[] pdfBytes = _invoiceService.GeneratePaymentReceiptPdf(payment, appointment);
                Console.WriteLine($"PDF generated, length: {pdfBytes?.Length ?? 0}");

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return BadRequest("Không thể tạo PDF - dữ liệu rỗng");
                }

                // Trả về file PDF
                string fileName = $"BienLai_ThanhToan_{payment.PaymentId}_{DateTime.Now:yyyyMMdd}.pdf";
                Console.WriteLine($"Returning file: {fileName}");
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in DownloadPaymentReceipt: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"Lỗi tạo biên lai: {ex.Message}");
            }
        }

                [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult TestPdf()
        {
            try
            {
                byte[] pdfBytes = _invoiceService.GenerateTestPdf();
                if (pdfBytes.Length == 0)
                {
                    return BadRequest("Không thể tạo PDF test");
                }
                return File(pdfBytes, "application/pdf", "test.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi tạo PDF test: {ex.Message}");
            }
        }

        private int? GetCurrentUserId()
        {
            // Thử lấy từ session (cách chính xác nhất)
            var sessionUserId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(sessionUserId) && int.TryParse(sessionUserId, out int sessionId))
            {
                return sessionId;
            }

            // Thử lấy từ claim UserID
            var userIdClaim = User.FindFirst("UserID");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            // Thử lấy từ claim NameIdentifier
            var nameIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (nameIdClaim != null && int.TryParse(nameIdClaim.Value, out int nameId))
            {
                return nameId;
            }

            return null;
        }
    }
} 