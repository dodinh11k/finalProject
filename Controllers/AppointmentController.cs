using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using test_2.Models;
using test_2.Services;
using System.Collections.Generic;
using System.Text.Json;

[Route("Appointment")]
public class AppointmentController : Controller
{
    private readonly MyGarageFinalContext _context;
    private readonly IEmailService _emailService;
    private readonly IVoucherService _voucherService;

    public AppointmentController(MyGarageFinalContext context, IEmailService emailService, IVoucherService voucherService)
    {
        _context = context;
        _emailService = emailService;
        _voucherService = voucherService;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View("~/Views/Appointment/Index.cshtml");
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var model = new AppointmentViewModel
        {
            AppointmentTime = DateTime.Now
        };

        await LoadDropdowns(model);
        return View("~/Views/Appointment/Create.cshtml", model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns(model);
            return View("~/Views/Appointment/Create.cshtml", model);
        }

        if (model.AppointmentTime <= DateTime.Now)
        {
            ModelState.AddModelError("AppointmentTime", "Thời gian hẹn phải nằm trong tương lai.");
            await LoadDropdowns(model);
            return View("~/Views/Appointment/Create.cshtml", model);
        }

        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdStr, out int userId))
        {
            TempData["Error"] = "Bạn cần đăng nhập để đặt lịch.";
            return RedirectToAction("Login", "AccountLogin");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            TempData["Error"] = "Không tìm thấy tài khoản.";
            return RedirectToAction("Login", "AccountLogin");
        }

        try
        {
            // Validate voucher if provided
            decimal discountAmount = 0;
            if (!string.IsNullOrWhiteSpace(model.PromoCode))
            {
                var services = await _context.Services
                    .Where(s => model.ServiceIds.Contains(s.ServiceId))
                    .ToListAsync();
                var totalServicePrice = services.Sum(s => s.Price ?? 0);
                
                var (isValid, message, discount) = await _voucherService.ValidateVoucherAsync(model.PromoCode, totalServicePrice);
                
                if (!isValid)
                {
                    ModelState.AddModelError("PromoCode", message);
                    await LoadDropdowns(model);
                    return View("~/Views/Appointment/Create.cshtml", model);
                }
                
                discountAmount = discount;
                TempData["VoucherMessage"] = message;
            }

            var vehicle = new Vehicle
            {
                UserId = user.UserId,
                Make = model.VehicleMake ?? "Không rõ",
                Model = model.VehicleModel ?? "Không rõ",
                LicensePlate = model.LicensePlate ?? "Chưa rõ",
                Year = DateTime.Now.Year,
                Notes = "Xe được thêm từ đặt lịch"
            };
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            var appointment = new Appointment
            {
                UserId = user.UserId,
                GarageId = model.GarageId,
                AppointmentTime = model.AppointmentTime,
                Notes = model.Notes,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                PromoCode = model.PromoCode,
                DiscountAmount = discountAmount
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            decimal basePrice = 0;
            if (model.ServiceIds != null && model.ServiceIds.Count > 0)
            {
                foreach (var serviceId in model.ServiceIds)
                {
                    var detail = new AppointmentVehicleDetail
                    {
                        AppointmentId = appointment.AppointmentId,
                        VehicleId = vehicle.VehicleId,
                        ServiceId = serviceId,
                        Quantity = 1,
                        Note = "Đặt lịch tự động"
                    };
                    _context.AppointmentVehicleDetails.Add(detail);
                    // Lấy giá dịch vụ
                    var service = await _context.Services.FirstOrDefaultAsync(s => s.ServiceId == serviceId);
                    if (service != null && service.Price.HasValue)
                        basePrice += service.Price.Value;
                }
                await _context.SaveChangesAsync();
            }

            // Lưu sản phẩm thay thế nếu có
            var replacementProductsJson = Request.Form["ReplacementProductsJson"].FirstOrDefault();
            if (!string.IsNullOrEmpty(replacementProductsJson))
            {
                try
                {
                    var replacementProducts = JsonSerializer.Deserialize<List<ReplacementProductItem>>(replacementProductsJson);
                    if (replacementProducts != null && replacementProducts.Count > 0)
                    {
                        foreach (var item in replacementProducts)
                        {
                            // Lấy giá sản phẩm tại thời điểm đặt
                            if (int.TryParse(item.ProductId, out int productId))
                            {
                                var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
                                decimal price = product?.Price ?? 0;
                                var detail = new AppointmentProductDetail
                                {
                                    AppointmentId = appointment.AppointmentId,
                                    ProductId = productId,
                                    Quantity = item.Quantity,
                                    UnitPrice = price
                                };
                                _context.AppointmentProductDetails.Add(detail);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch { /* ignore parse error */ }
            }

            // --- TÍNH GIẢM GIÁ ---
            // 1. Khách hàng thân thiết (từ lần 2 trở đi)
            int previousAppointments = await _context.Appointments.CountAsync(a => a.UserId == user.UserId && a.AppointmentId != appointment.AppointmentId);
            decimal loyaltyDiscount = previousAppointments >= 1 ? basePrice * 0.05m : 0; // 5%

            // 2. Số lượng xe (1 xe nên không giảm, nếu muốn có thể set = 1)
            decimal carCountDiscount = 0; // Đặt lịch 1 xe, không giảm

            // 3. Mã khuyến mãi (sử dụng VoucherService)
            decimal promoDiscount = 0;
            if (!string.IsNullOrEmpty(model.PromoCode))
            {
                // Sử dụng VoucherService để validate và tính toán
                var (isValid, message, voucherDiscount) = await _voucherService.ValidateVoucherAsync(model.PromoCode, basePrice);
                
                if (isValid)
                {
                    promoDiscount = voucherDiscount;
                    TempData["VoucherMessage"] = message;
                }
                else
                {
                    // Fallback cho các mã cũ nếu cần
                    if (model.PromoCode.ToUpper() == "WELCOME10")
                        promoDiscount = basePrice * 0.10m; // 10%
                    else if (model.PromoCode.ToUpper() == "SUMMER2025")
                        promoDiscount = 50000; // Giảm 50k
                }
            }

            decimal totalDiscount = loyaltyDiscount + carCountDiscount + promoDiscount;
            decimal totalAmount = basePrice - totalDiscount;
            if (totalAmount < 0) totalAmount = 0;

            appointment.PromoCode = model.PromoCode;
            appointment.DiscountAmount = totalDiscount;
            appointment.TotalAmount = totalAmount;
            await _context.SaveChangesAsync();

            var serviceNames = await _context.Services
                .Where(s => model.ServiceIds.Contains(s.ServiceId))
                .Select(s => s.ServiceName)
                .ToListAsync();
            var garage = await _context.Garages.FirstOrDefaultAsync(g => g.GarageId == model.GarageId);
            var technicianName = "Chưa phân công";

            if (!string.IsNullOrEmpty(user.Email))
            {
                try
                {
                    await _emailService.SendAppointmentConfirmationEmailAsync(
                        user.Email,
                        user.FullName ?? user.Username,
                        appointment.AppointmentId.ToString(),
                        appointment.AppointmentTime ?? DateTime.Now,
                        string.Join(", ", serviceNames),
                        garage?.Address ?? "Không xác định",
                        technicianName
                    );
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"Lỗi gửi email: {emailEx.Message}");
                }
            }

            return RedirectToAction("Details", "Appointment", new { id = appointment.AppointmentId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi hệ thống: " + ex.ToString());
            await LoadDropdowns(model);
            return View("~/Views/Appointment/Create.cshtml", model);
        }
    }

    private async Task LoadDropdowns(AppointmentViewModel model)
    {
        model.ServiceList = await _context.Services
            .Select(s => new SelectListItem
            {
                Value = s.ServiceId.ToString(),
                Text = s.ServiceName
            }).ToListAsync();

        model.GarageList = await _context.Garages
            .Select(g => new SelectListItem
            {
                Value = g.GarageId.ToString(),
                Text = g.Address
            }).ToListAsync();
    }

    private async Task LoadDropdowns(AppointmentEditViewModel model)
    {
        model.GarageList = await _context.Garages
            .Select(g => new SelectListItem
            {
                Value = g.GarageId.ToString(),
                Text = g.Address
            }).ToListAsync();

        // Nạp danh sách dịch vụ
        model.ServiceList = await _context.Services
            .Select(s => new SelectListItem
            {
                Value = s.ServiceId.ToString(),
                Text = s.ServiceName
            }).ToListAsync();

        // Nạp danh sách xe của user (nếu cần)
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (int.TryParse(userIdStr, out int userId))
        {
            model.VehicleList = await _context.Vehicles
                .Where(v => v.UserId == userId)
                .Select(v => new SelectListItem
                {
                    Value = v.VehicleId.ToString(),
                    Text = (v.Make ?? "") + " " + (v.Model ?? "") + " (" + (v.LicensePlate ?? "") + ")"
                }).ToListAsync();
        }
    }

    // Action cho đặt lịch nhiều xe
    [HttpGet("CreateMulti")]
    public async Task<IActionResult> CreateMulti()
    {
        var model = new MultiVehicleAppointmentViewModel
        {
            AppointmentTime = DateTime.Now,
            Vehicles = new List<VehicleAppointmentItem> { new VehicleAppointmentItem() } // Bắt đầu với 1 xe
        };

        await LoadDropdownsForMulti(model);
        return View("~/Views/Appointment/CreateMulti.cshtml", model);
    }

    [HttpPost("CreateMulti")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMulti(MultiVehicleAppointmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsForMulti(model);
            return View("~/Views/Appointment/CreateMulti.cshtml", model);
        }

        if (model.AppointmentTime <= DateTime.Now)
        {
            ModelState.AddModelError("AppointmentTime", "Thời gian hẹn phải nằm trong tương lai.");
            await LoadDropdownsForMulti(model);
            return View("~/Views/Appointment/CreateMulti.cshtml", model);
        }

        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdStr, out int userId))
        {
            TempData["Error"] = "Bạn cần đăng nhập để đặt lịch.";
            return RedirectToAction("Login", "AccountLogin");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            TempData["Error"] = "Không tìm thấy tài khoản.";
            return RedirectToAction("Login", "AccountLogin");
        }

        try
        {
            // Tạo appointment chính
            var appointment = new Appointment
            {
                UserId = user.UserId,
                GarageId = model.GarageId,
                AppointmentTime = model.AppointmentTime,
                Notes = model.Notes,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var allServiceNames = new List<string>();
            decimal basePrice = 0;
            int carCount = model.Vehicles.Count;

            // Parse và lưu sản phẩm thay thế cho từng xe
            var replacementProductsMultiJson = Request.Form["ReplacementProductsMultiJson"].FirstOrDefault();
            List<List<ReplacementProductItem>> replacementProductsMulti = null;
            
            // Debug: Log JSON data for multi-vehicle replacement products
            Console.WriteLine($"ReplacementProductsMultiJson: {replacementProductsMultiJson}");
            
            if (!string.IsNullOrEmpty(replacementProductsMultiJson))
            {
                try
                {
                    replacementProductsMulti = System.Text.Json.JsonSerializer.Deserialize<List<List<ReplacementProductItem>>>(replacementProductsMultiJson);
                    Console.WriteLine($"Deserialized replacementProductsMulti count: {replacementProductsMulti?.Count ?? 0}");
                    if (replacementProductsMulti != null)
                    {
                        for (int i = 0; i < replacementProductsMulti.Count; i++)
                        {
                            Console.WriteLine($"Vehicle {i} has {replacementProductsMulti[i]?.Count ?? 0} replacement products");
                        }
                    }
                }
                catch (Exception ex) 
                { 
                    Console.WriteLine($"Lỗi xử lý ReplacementProductsMultiJson: {ex.Message}");
                    replacementProductsMulti = null; 
                }
            }
            int vehicleIdx = 0;
            // Xử lý từng xe
            foreach (var vehicleItem in model.Vehicles)
            {
                Vehicle vehicle;
                if (vehicleItem.SelectedVehicleId.HasValue)
                {
                    vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleItem.SelectedVehicleId.Value);
                    if (vehicle == null)
                    {
                        throw new Exception($"Không tìm thấy xe với ID: {vehicleItem.SelectedVehicleId.Value}");
                    }
                }
                else
                {
                    vehicle = new Vehicle
                    {
                        UserId = user.UserId,
                        Make = vehicleItem.VehicleMake ?? "Không rõ",
                        Model = vehicleItem.VehicleModel ?? "Không rõ",
                        LicensePlate = vehicleItem.LicensePlate ?? "Chưa rõ",
                        Year = DateTime.Now.Year,
                        Notes = "Xe được thêm từ đặt lịch nhiều xe"
                    };
                    _context.Vehicles.Add(vehicle);
                    await _context.SaveChangesAsync();
                }
                // Thêm các dịch vụ cho xe này
                if (vehicleItem.ServiceIds != null && vehicleItem.ServiceIds.Count > 0)
                {
                    foreach (var serviceId in vehicleItem.ServiceIds)
                    {
                        var detail = new AppointmentVehicleDetail
                        {
                            AppointmentId = appointment.AppointmentId,
                            VehicleId = vehicle.VehicleId,
                            ServiceId = serviceId,
                            Quantity = 1,
                            Note = vehicleItem.VehicleNotes ?? "Đặt lịch tự động"
                        };
                        _context.AppointmentVehicleDetails.Add(detail);
                        // Lấy giá dịch vụ
                        var service = await _context.Services.FirstOrDefaultAsync(s => s.ServiceId == serviceId);
                        if (service != null && service.Price.HasValue)
                            basePrice += service.Price.Value;
                    }
                    await _context.SaveChangesAsync();
                }
                // Lưu sản phẩm thay thế cho xe này nếu có
                if (replacementProductsMulti != null && vehicleIdx < replacementProductsMulti.Count)
                {
                    var vehicleReplacementProducts = replacementProductsMulti[vehicleIdx];
                    Console.WriteLine($"Processing replacement products for vehicle {vehicleIdx}: {vehicleReplacementProducts?.Count ?? 0} products");
                    
                    if (vehicleReplacementProducts != null && vehicleReplacementProducts.Count > 0)
                    {
                        foreach (var item in vehicleReplacementProducts)
                        {
                            if (int.TryParse(item.ProductId, out int productId))
                            {
                                var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
                                decimal price = product?.Price ?? 0;
                                var detail = new AppointmentProductDetail
                                {
                                    AppointmentId = appointment.AppointmentId,
                                    ProductId = productId,
                                    Quantity = item.Quantity,
                                    UnitPrice = price
                                };
                                _context.AppointmentProductDetails.Add(detail);
                                Console.WriteLine($"Added vehicle-specific product: Vehicle={vehicleIdx}, ProductId={productId}, Quantity={item.Quantity}, Price={price}");
                            }
                            else
                            {
                                Console.WriteLine($"Invalid ProductId for vehicle {vehicleIdx}: {item.ProductId}");
                            }
                        }
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"Saved {vehicleReplacementProducts.Count} replacement products for vehicle {vehicleIdx}");
                    }
                }
                vehicleIdx++;
            }



            // --- TÍNH GIẢM GIÁ ---
            // 1. Khách hàng thân thiết (từ lần 2 trở đi)
            int previousAppointments = await _context.Appointments.CountAsync(a => a.UserId == user.UserId && a.AppointmentId != appointment.AppointmentId);
            decimal loyaltyDiscount = previousAppointments >= 1 ? basePrice * 0.05m : 0; // 5%

            // 2. Số lượng xe (>=2)
            decimal carCountDiscount = carCount >= 2 ? basePrice * 0.10m : 0; // 10%

            // 3. Mã khuyến mãi (sử dụng VoucherService)
            decimal promoDiscount = 0;
            if (!string.IsNullOrEmpty(model.PromoCode))
            {
                // Sử dụng VoucherService để validate và tính toán
                var (isValid, message, voucherDiscount) = await _voucherService.ValidateVoucherAsync(model.PromoCode, basePrice);
                
                if (isValid)
                {
                    promoDiscount = voucherDiscount;
                    TempData["VoucherMessage"] = message;
                }
                else
                {
                    // Fallback cho các mã cũ nếu cần
                    if (model.PromoCode.ToUpper() == "WELCOME10")
                        promoDiscount = basePrice * 0.10m; // 10%
                    else if (model.PromoCode.ToUpper() == "SUMMER2025")
                        promoDiscount = 50000; // Giảm 50k
                }
            }

            // Tính tổng tiền sản phẩm thay thế từ tất cả các xe
            decimal totalProductPrice = 0;
            if (replacementProductsMulti != null)
            {
                foreach (var vehicleProducts in replacementProductsMulti)
                {
                    if (vehicleProducts != null)
                    {
                        foreach (var item in vehicleProducts)
                        {
                            if (int.TryParse(item.ProductId, out int productId))
                            {
                                var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
                                decimal price = product?.Price ?? 0;
                                totalProductPrice += price * item.Quantity;
                            }
                        }
                    }
                }
            }
            
            decimal totalDiscount = loyaltyDiscount + carCountDiscount + promoDiscount;
            decimal totalAmount = basePrice + totalProductPrice - totalDiscount;
            if (totalAmount < 0) totalAmount = 0;

            appointment.PromoCode = model.PromoCode;
            appointment.DiscountAmount = totalDiscount;
            appointment.TotalAmount = totalAmount;
            await _context.SaveChangesAsync();

            // Gửi email xác nhận
            var garage = await _context.Garages.FirstOrDefaultAsync(g => g.GarageId == model.GarageId);
            var technicianName = "Chưa phân công";

            if (!string.IsNullOrEmpty(user.Email))
            {
                try
                {
                    await _emailService.SendAppointmentConfirmationEmailAsync(
                        user.Email,
                        user.FullName ?? user.Username,
                        appointment.AppointmentId.ToString(),
                        appointment.AppointmentTime ?? DateTime.Now,
                        string.Join(", ", allServiceNames.Distinct()),
                        garage?.Address ?? "Không xác định",
                        technicianName
                    );
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"Lỗi gửi email: {emailEx.Message}");
                }
            }

            TempData["SuccessMessage"] = $"Đã đặt lịch thành công cho {model.Vehicles.Count} xe!";
            return RedirectToAction("Details", "Appointment", new { id = appointment.AppointmentId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi hệ thống: " + ex.ToString());
            await LoadDropdownsForMulti(model);
            return View("~/Views/Appointment/CreateMulti.cshtml", model);
        }
    }

    private async Task LoadDropdownsForMulti(MultiVehicleAppointmentViewModel model)
    {
        model.ServiceList = await _context.Services
            .Select(s => new SelectListItem
            {
                Value = s.ServiceId.ToString(),
                Text = s.ServiceName
            }).ToListAsync();

        model.GarageList = await _context.Garages
            .Select(g => new SelectListItem
            {
                Value = g.GarageId.ToString(),
                Text = g.Address
            }).ToListAsync();

        // Lấy danh sách xe của user hiện tại
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (int.TryParse(userIdStr, out int userId))
        {
            model.VehicleList = await _context.Vehicles
                .Where(v => v.UserId == userId)
                .Select(v => new SelectListItem
                {
                    Value = v.VehicleId.ToString(),
                    Text = $"{v.Make} {v.Model} - {v.LicensePlate}"
                }).ToListAsync();
        }

        // Load danh sách hãng xe
        model.VehicleMakeList = VehicleData.GetVehicleMakeList();
        model.ProductList = await _context.Products
            .Select(p => new SelectListItem
            {
                Value = p.ProductId.ToString(),
                Text = p.ProductName + (p.Price > 0 ? $" - {p.Price:N0} đ" : "")
            }).ToListAsync();
    }

    [HttpGet("GetVehicleModels")]
    public IActionResult GetVehicleModels(string make)
    {
        var models = VehicleData.GetVehicleModelList(make);
        return Json(models);
    }

    [HttpPost("ValidateVoucher")]
    public async Task<IActionResult> ValidateVoucher(string promoCode, decimal totalAmount)
    {
        if (string.IsNullOrWhiteSpace(promoCode))
        {
            return Json(new { isValid = false, message = "Mã khuyến mãi không được để trống" });
        }

        var (isValid, message, discountAmount) = await _voucherService.ValidateVoucherAsync(promoCode, totalAmount);
        var discountedAmount = totalAmount - discountAmount;

        return Json(new 
        { 
            isValid, 
            message, 
            discountAmount, 
            discountedAmount,
            originalAmount = totalAmount
        });
    }

    [HttpGet("Edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "AccountLogin");

        var appointment = await _context.Appointments
            .Include(a => a.AppointmentVehicleDetails).ThenInclude(d => d.Vehicle)
            .FirstOrDefaultAsync(a => a.AppointmentId == id && a.UserId == userId);

        if (appointment == null) return NotFound();

        var detail = appointment.AppointmentVehicleDetails.FirstOrDefault();
        if (detail == null) return NotFound();

        var vehicle = detail.Vehicle;

        var model = new AppointmentEditViewModel
        {
            AppointmentId = appointment.AppointmentId,
            AppointmentTime = appointment.AppointmentTime ?? DateTime.Now,
            Notes = appointment.Notes,
            GarageId = appointment.GarageId ?? 0,
            VehicleMake = vehicle?.Make,
            VehicleModel = vehicle?.Model,
            LicensePlate = vehicle?.LicensePlate,
            // Nạp các trường mới
            ServiceIds = appointment.AppointmentVehicleDetails.Select(d => d.ServiceId ?? 0).Where(id => id != 0).ToList(),
            PromoCode = appointment.PromoCode,
            SelectedVehicleId = vehicle?.VehicleId
        };

        await LoadDropdowns(model);
        return View("~/Views/Appointment/Edit.cshtml", model);
    }

    [HttpPost("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AppointmentEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns(model);
            return View("~/Views/Appointment/Edit.cshtml", model);
        }

        if (model.AppointmentTime <= DateTime.Now)
        {
            ModelState.AddModelError("AppointmentTime", "Thời gian hẹn phải nằm trong tương lai.");
            await LoadDropdowns(model);
            return View("~/Views/Appointment/Edit.cshtml", model);
        }

        var appointment = await _context.Appointments
            .Include(a => a.AppointmentVehicleDetails)
            .FirstOrDefaultAsync(a => a.AppointmentId == model.AppointmentId);

        if (appointment == null) return NotFound();

        var detail = appointment.AppointmentVehicleDetails.FirstOrDefault();
        if (detail == null) return NotFound();

        var vehicle = await _context.Vehicles.FindAsync(detail.VehicleId);
        if (vehicle != null)
        {
            vehicle.Make = model.VehicleMake;
            vehicle.Model = model.VehicleModel;
            vehicle.LicensePlate = model.LicensePlate;
        }

        appointment.AppointmentTime = model.AppointmentTime;
        appointment.Notes = model.Notes;
        appointment.GarageId = model.GarageId;

        // Cập nhật lại các dịch vụ cho lịch hẹn
        if (model.ServiceIds != null && model.ServiceIds.Count > 0)
        {
            // Xóa các dịch vụ cũ
            _context.AppointmentVehicleDetails.RemoveRange(appointment.AppointmentVehicleDetails);
            // Thêm lại các dịch vụ mới
            foreach (var serviceId in model.ServiceIds)
            {
                var newDetail = new AppointmentVehicleDetail
                {
                    AppointmentId = appointment.AppointmentId,
                    VehicleId = detail.VehicleId,
                    ServiceId = serviceId,
                    CreatedAt = DateTime.Now
                };
                _context.AppointmentVehicleDetails.Add(newDetail);
            }
        }
        // Cập nhật mã khuyến mãi
        appointment.PromoCode = model.PromoCode;

        await _context.SaveChangesAsync();
        return RedirectToAction("History");
    }

    [HttpGet("EditMulti")]
    public async Task<IActionResult> EditMulti(int id)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "AccountLogin");

        var appointment = await _context.Appointments
            .Include(a => a.AppointmentVehicleDetails).ThenInclude(d => d.Vehicle)
            .FirstOrDefaultAsync(a => a.AppointmentId == id && a.UserId == userId);
        if (appointment == null) return NotFound();

        // Nạp danh sách các xe và dịch vụ cho từng xe
        var vehicles = appointment.AppointmentVehicleDetails
            .GroupBy(d => d.VehicleId)
            .Select(g =>
            {
                var first = g.First();
                var vehicle = first.Vehicle;
                return new VehicleAppointmentItem
                {
                    SelectedVehicleId = vehicle?.VehicleId,
                    VehicleMake = vehicle?.Make,
                    VehicleModel = vehicle?.Model,
                    LicensePlate = vehicle?.LicensePlate,
                    ServiceIds = g.Select(d => d.ServiceId ?? 0).Where(sid => sid != 0).ToList(),
                    VehicleNotes = vehicle?.Notes
                };
            }).ToList();

        var model = new MultiVehicleAppointmentViewModel
        {
            GarageId = appointment.GarageId ?? 0,
            AppointmentTime = appointment.AppointmentTime ?? DateTime.Now,
            Notes = appointment.Notes,
            PromoCode = appointment.PromoCode,
            Vehicles = vehicles
        };

        await DropdownHelper.LoadDropdownsForMulti(_context, model, userId);
        return View("~/Views/Appointment/EditMulti.cshtml", model);
    }

    [HttpPost("EditMulti")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMulti(int id, MultiVehicleAppointmentViewModel model)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdStr, out int userId))
        {
            TempData["Error"] = "Bạn cần đăng nhập để chỉnh sửa lịch hẹn.";
            return RedirectToAction("Login", "AccountLogin");
        }
        // Không xử lý dịch vụ nữa, chỉ cập nhật các trường thông tin khác
        var existingAppointment = await _context.Appointments
            .Include(a => a.AppointmentVehicleDetails)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);
        if (existingAppointment == null) return NotFound();
        // Không xóa hoặc cập nhật lại dịch vụ!
        existingAppointment.GarageId = model.GarageId;
        existingAppointment.AppointmentTime = model.AppointmentTime;
        existingAppointment.Notes = model.Notes;
        existingAppointment.PromoCode = model.PromoCode;
        await _context.SaveChangesAsync();
        return RedirectToAction("History");
    }

    [HttpGet("Cancel/{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdStr, out int userId))
            return RedirectToAction("Login", "AccountLogin");

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentId == id && a.UserId == userId);

        if (appointment == null)
            return NotFound();

        if (appointment.Status == "Completed" || appointment.Status == "Canceled")
        {
            TempData["Error"] = "Lịch hẹn này không thể hủy.";
            return RedirectToAction("History");
        }

        appointment.Status = "Canceled";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Lịch hẹn đã được hủy.";
        return RedirectToAction("History");
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Garage)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);

        if (appointment == null)
            return NotFound();

        var vehicleDetails = await _context.AppointmentVehicleDetails
            .Include(d => d.Vehicle)
            .Include(d => d.Service)
            .Where(d => d.AppointmentId == id)
            .ToListAsync();

        // Tính tổng tiền dịch vụ
        decimal? totalServicePriceNullable = vehicleDetails.Sum(d => (d.Service?.Price ?? 0m) * d.Quantity);
        decimal totalServicePrice = totalServicePriceNullable ?? 0m;
        
        // Lấy sản phẩm thay thế
        var replacementProducts = await _context.AppointmentProductDetails
            .Include(p => p.Product)
            .Where(p => p.AppointmentId == id)
            .ToListAsync();
        decimal totalProductPrice = replacementProducts.Sum(p => p.UnitPrice * p.Quantity);
        
        decimal discountAmount = appointment.DiscountAmount.HasValue ? appointment.DiscountAmount.Value : 0m;
        decimal totalAmount = appointment.TotalAmount.HasValue ? appointment.TotalAmount.Value : (totalServicePrice + totalProductPrice - discountAmount);
        if (totalAmount < 0) totalAmount = 0;
        
        // Cập nhật lại tổng tiền nếu chưa có
        if (!appointment.TotalAmount.HasValue)
        {
            appointment.TotalAmount = totalAmount;
            await _context.SaveChangesAsync();
        }

        // Chi tiết giá từng dịch vụ
        var servicePriceDetails = vehicleDetails
            .Where(d => d.Service != null)
            .GroupBy(d => d.Service.ServiceName)
            .Select(g => (ServiceName: g.Key, Price: g.Sum(d => (d.Service.Price ?? 0m) * d.Quantity) ?? 0m))
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
                promoDiscount = 50000;
            }
            else
            {
                // Sử dụng VoucherService để lấy thông tin voucher
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

        var paymentSuccess = await _context.PaymentHistories.AnyAsync(p => p.AppointmentId == appointment.AppointmentId && p.Status == "Success");
        ViewBag.PaymentSuccess = paymentSuccess;

        return View("~/Views/Appointment/Details.cshtml", viewModel);
    }

    [HttpGet("History")]
    public async Task<IActionResult> History()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");

        if (!int.TryParse(userIdStr, out int userId))
        {
            HttpContext.Session.SetString("ReturnUrl", Url.Action("History", "Appointment"));
            return RedirectToAction("Login", "AccountLogin");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            return RedirectToAction("Login", "AccountLogin");
        }

        // ✅ Hiển thị những appointment đã được xác nhận và những appointment đã thanh toán (Pending nhưng có payment success)
        var appointments = await _context.Appointments
            .Where(a => a.UserId == user.UserId && 
                       (a.Status == "Confirmed" || a.Status == "Completed" || a.Status == "In Progress" || a.Status == "In_Progress"))
            .Include(a => a.Garage)
            .OrderByDescending(a => a.AppointmentTime)
            .ToListAsync();

        // ✅ Thêm những appointment có status "Pending" nhưng đã thanh toán thành công
        var pendingAppointments = await _context.Appointments
            .Where(a => a.UserId == user.UserId && a.Status == "Pending")
            .Include(a => a.Garage)
            .ToListAsync();

        var pendingAppointmentIds = pendingAppointments.Select(a => a.AppointmentId).ToList();
        var successfulPayments = await _context.PaymentHistories
            .Where(p => pendingAppointmentIds.Contains(p.AppointmentId ?? 0) && p.Status == "Success")
            .Select(p => p.AppointmentId)
            .ToListAsync();

        var paidPendingAppointments = pendingAppointments
            .Where(a => successfulPayments.Contains(a.AppointmentId))
            .OrderByDescending(a => a.AppointmentTime)
            .ToList();

        // ✅ Kết hợp tất cả appointments
        appointments.AddRange(paidPendingAppointments);
        appointments = appointments.OrderByDescending(a => a.AppointmentTime).ToList();

        var appointmentIds = appointments.Select(a => a.AppointmentId).ToList();

        var allDetails = await _context.AppointmentVehicleDetails
            .Where(d => appointmentIds.Contains(d.AppointmentId))
            .Include(d => d.Vehicle)
            .Include(d => d.Service)
            .Include(d => d.Technician)
            .ToListAsync();

        var allReviews = await _context.Reviews
    .Where(r => appointmentIds.Contains(r.AppointmentId))
    .ToListAsync();

        var payments = await _context.PaymentHistories
            .Where(p => appointmentIds.Contains(p.AppointmentId ?? 0))
            .ToListAsync();

        var viewModel = new AppointmentHistoryViewModel
        {
            Appointments = appointments,
            Details = allDetails,
            Reviews = allReviews,
            Payments = payments
        };

        return View("~/Views/Appointment/History.cshtml", viewModel);
    }

    [HttpGet("AllAppointments")]
    public async Task<IActionResult> AllAppointments()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");

        if (!int.TryParse(userIdStr, out int userId))
        {
            HttpContext.Session.SetString("ReturnUrl", Url.Action("AllAppointments", "Appointment"));
            return RedirectToAction("Login", "AccountLogin");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            return RedirectToAction("Login", "AccountLogin");
        }

        // ✅ Hiển thị tất cả appointments (bao gồm cả Pending)
        var appointments = await _context.Appointments
            .Where(a => a.UserId == user.UserId)
            .Include(a => a.Garage)
            .OrderByDescending(a => a.AppointmentTime)
            .ToListAsync();

        var appointmentIds = appointments.Select(a => a.AppointmentId).ToList();

        var allDetails = await _context.AppointmentVehicleDetails
            .Where(d => appointmentIds.Contains(d.AppointmentId))
            .Include(d => d.Vehicle)
            .Include(d => d.Service)
            .Include(d => d.Technician)
            .ToListAsync();

        // ✅ Lấy thông tin sản phẩm thay thế
        var replacementProducts = await _context.AppointmentProductDetails
            .Where(p => appointmentIds.Contains(p.AppointmentId))
            .Include(p => p.Product)
            .ToListAsync();

        // ✅ Lấy thông tin payment để hiển thị trạng thái thanh toán
        var payments = await _context.PaymentHistories
            .Where(p => appointmentIds.Contains(p.AppointmentId ?? 0))
            .ToListAsync();


        var viewModel = new AppointmentHistoryViewModel
        {
            Appointments = appointments,
            Details = allDetails,
            ReplacementProducts = replacementProducts
        };

        ViewBag.Payments = payments;

        return View("~/Views/Appointment/AllAppointments.cshtml", viewModel);
    }

    [HttpGet("Review/{id}")]
    public async Task<IActionResult> Review(int id)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdStr, out int userId))
            return RedirectToAction("Login", "AccountLogin");

        var appointment = await _context.Appointments
            .Include(a => a.Garage)
            .FirstOrDefaultAsync(a => a.AppointmentId == id && a.UserId == userId);

        if (appointment == null || appointment.Status != "Completed")
            return NotFound();

        // Kiểm tra đã thanh toán thành công chưa
        var hasPaid = await _context.PaymentHistories.AnyAsync(p => p.AppointmentId == id && p.Status == "Success");
        if (!hasPaid)
        {
            ViewBag.PaymentRequired = true;
            return View("~/Views/Appointment/Review.cshtml", appointment);
        }

        return View("~/Views/Appointment/Review.cshtml", appointment);
    }

    [HttpPost("Review/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, int Rating, string Content)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdStr, out int userId))
            return RedirectToAction("Login", "AccountLogin");

        var appointment = await _context.Appointments
            .Include(a => a.Garage)
            .FirstOrDefaultAsync(a => a.AppointmentId == id && a.UserId == userId);

        if (appointment == null || appointment.Status != "Completed")
            return NotFound();

        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.AppointmentId == appointment.AppointmentId);

        if (review == null)
        {
            review = new Review
            {
                UserId = userId,
                GarageId = appointment.GarageId,
                AppointmentId = appointment.AppointmentId,
                Rating = Rating,
                Comment = Content,
                CreatedAt = DateTime.Now
            };
            _context.Reviews.Add(review);
        }
        else
        {
            review.Rating = Rating;
            review.Comment = Content;
            review.CreatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        // Sau khi lưu review thành công:
        if (appointment != null)
        {
            var customerName = appointment.User?.FullName;
            if (string.IsNullOrWhiteSpace(customerName))
                customerName = appointment.User?.Username ?? "Không rõ";
            System.Diagnostics.Debug.WriteLine($"[Review] UserId={appointment.User?.UserId}, FullName={appointment.User?.FullName}, Username={appointment.User?.Username}, customerName={customerName}");
            var notification = new Notification
            {
                Title = "Đánh giá mới từ khách hàng",
                Message = $"Khách hàng {customerName} vừa đánh giá lịch hẹn #{id} với {Rating} sao.",
                CreatedAt = DateTime.Now,
                IsRead = false,
                UserId = null // Thông báo cho admin, không gắn user cụ thể
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        TempData["Success"] = "Cảm ơn bạn đã đánh giá!";
        return RedirectToAction("History");
    }

}
