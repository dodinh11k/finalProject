using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using test_2.Models;

namespace test_2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly MyGarageFinalContext _context;

        public AdminController(MyGarageFinalContext context)
        {
            _context = context;
        }

        // Thêm hàm dùng chung để load noti
        private async Task LoadAdminNotifications()
        {
            ViewBag.AllProducts = await _context.Products.ToListAsync();
            var adminNotifications = await _context.Notifications
                .Where(n => n.UserId == null && (n.IsRead == false || n.IsRead == null))
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();
            ViewBag.AdminNotifications = adminNotifications;
            ViewBag.AdminNotificationCount = adminNotifications.Count;
        }

        // --- USER MANAGEMENT ---
        public async Task<IActionResult> Users(string role = null)
        {
            await LoadAdminNotifications();
            var users = _context.Users.AsQueryable();
            if (!string.IsNullOrEmpty(role))
                users = users.Where(u => u.Role == role);
            return View(await users.ToListAsync());
        }

        public async Task<IActionResult> CreateUser()
        {
            await LoadAdminNotifications();
            return View(new User { IsActive = true, Username = "", PasswordHash = "" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User user)
        {
            await LoadAdminNotifications();
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Users");
            }
            return View(user);
        }

        public async Task<IActionResult> EditUser(int id)
        {
            await LoadAdminNotifications();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }
        public async Task<IActionResult> DeleteUser(int id)
        {
            await LoadAdminNotifications();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (user.Role == "Admin")
            {
                TempData["Error"] = "Không thể xóa tài khoản Admin.";
                return RedirectToAction("Users");
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa người dùng thành công.";
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(User user)
        {
            await LoadAdminNotifications();
            
            // Clear PasswordHash validation error if it's empty (optional during edit)
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                ModelState.Remove("PasswordHash");
            }
            
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);
            if (userInDb == null) return NotFound();

            userInDb.Username = user.Username;

            if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                userInDb.PasswordHash = user.PasswordHash;
            }

            userInDb.FullName = user.FullName;
            userInDb.Email = user.Email;
            userInDb.Phone = user.Phone;
            userInDb.Address = user.Address;
            userInDb.Role = user.Role;
            userInDb.IsActive = user.IsActive ?? false; // Đảm bảo không null

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thông tin người dùng thành công.";
                return RedirectToAction("Users");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
                user.IsActive = userInDb.IsActive ?? false;
                return View(user);
            }
        }

        // --- SERVICE MANAGEMENT ---
        public async Task<IActionResult> Services()
        {
            await LoadAdminNotifications();
            var services = await _context.Services.ToListAsync();
            return View(services);
        }

        public async Task<IActionResult> CreateService() { await LoadAdminNotifications(); return View(); }

        [HttpPost]
        public async Task<IActionResult> CreateService(Service service)
        {
            await LoadAdminNotifications();
            if (ModelState.IsValid)
            {
                _context.Services.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction("Services");
            }
            return View(service);
        }

        public async Task<IActionResult> EditService(int id)
        {
            await LoadAdminNotifications();
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();
            return View(service);
        }

        [HttpPost]
        public async Task<IActionResult> EditService(Service service)
        {
            await LoadAdminNotifications();
            if (ModelState.IsValid)
            {
                _context.Update(service);
                await _context.SaveChangesAsync();
                return RedirectToAction("Services");
            }
            return View(service);
        }

        public async Task<IActionResult> DeleteService(int id)
        {
            await LoadAdminNotifications();
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction("Services");
        }

        // --- APPOINTMENT (ORDER) MANAGEMENT ---
        public async Task<IActionResult> Orders(string customerName, string phone, DateTime? date, string vehicleKeyword)
        {
            await LoadAdminNotifications();
            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Garage)
                .Include(a => a.AppointmentVehicleDetails).ThenInclude(d => d.Vehicle)
                .Include(a => a.AppointmentVehicleDetails).ThenInclude(d => d.Service)
                .AsQueryable();

            if (!string.IsNullOrEmpty(customerName))
                query = query.Where(a => a.User.FullName.Contains(customerName));

            if (!string.IsNullOrEmpty(phone))
                query = query.Where(a => a.User.Phone.Contains(phone));

            if (date.HasValue)
                query = query.Where(a => a.AppointmentTime.HasValue && a.AppointmentTime.Value.Date == date.Value.Date);

            if (!string.IsNullOrEmpty(vehicleKeyword))
                query = query.Where(a => a.AppointmentVehicleDetails.Any(d =>
                    d.Vehicle.Make.Contains(vehicleKeyword) ||
                    d.Vehicle.Model.Contains(vehicleKeyword) ||
                    d.Vehicle.LicensePlate.Contains(vehicleKeyword)));

            var orders = await query.OrderByDescending(a => a.AppointmentTime).ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> EditOrder(int id)
        {
            await LoadAdminNotifications();
            var order = await _context.Appointments
                .Include(a => a.AppointmentVehicleDetails)
                    .ThenInclude(d => d.Vehicle)
                .Include(a => a.AppointmentVehicleDetails)
                    .ThenInclude(d => d.Service)
                .Include(a => a.AppointmentVehicleDetails)
                    .ThenInclude(d => d.Technician)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (order == null) return NotFound();

            var technicians = await _context.Users.Where(u => u.Role == "Technician").ToListAsync();
            ViewBag.Technicians = technicians;
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> EditOrder(int id, int? technicianId, string status, string notes)
        {
            await LoadAdminNotifications();
            var order = await _context.Appointments
                .Include(a => a.AppointmentVehicleDetails)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (order == null) return NotFound();

            order.Status = status;
            order.Notes = notes;

            foreach (var detail in order.AppointmentVehicleDetails)
            {
                detail.TechnicianId = technicianId;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Orders");
        }

        public async Task<IActionResult> DeleteOrder(int id)
        {
            await LoadAdminNotifications();
            var appointment = await _context.Appointments
                .Include(a => a.AppointmentVehicleDetails)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return NotFound();

            var repairStatuses = await _context.RepairStatuses.Where(r => r.AppointmentId == id).ToListAsync();
            _context.RepairStatuses.RemoveRange(repairStatuses);

            var technicalReports = await _context.TechnicalReports.Where(t => t.AppointmentId == id).ToListAsync();
            _context.TechnicalReports.RemoveRange(technicalReports);

            var appointmentDetails = await _context.AppointmentVehicleDetails.Where(d => d.AppointmentId == id).ToListAsync();
            _context.AppointmentVehicleDetails.RemoveRange(appointmentDetails);

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa lịch hẹn và toàn bộ dữ liệu liên quan.";
            return RedirectToAction("Orders");
        }

        // --- PRODUCT MANAGEMENT ---
        public async Task<IActionResult> Products()
        {
            await LoadAdminNotifications();
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> CreateProduct()
        {
            await LoadAdminNotifications();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            await LoadAdminNotifications();
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.Now;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Products");
            }
            return View(product);
        }

        public async Task<IActionResult> EditProduct(int id)
        {
            await LoadAdminNotifications();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product product)
        {
            await LoadAdminNotifications();
            if (ModelState.IsValid)
            {
                var productInDb = await _context.Products.FindAsync(product.ProductId);
                if (productInDb == null) return NotFound();
                productInDb.ProductName = product.ProductName;
                productInDb.Description = product.Description;
                productInDb.Price = product.Price;
                productInDb.StockQuantity = product.StockQuantity;
                productInDb.ImageUrl = product.ImageUrl;
                await _context.SaveChangesAsync();
                return RedirectToAction("Products");
            }
            return View(product);
        }

        public async Task<IActionResult> DeleteProduct(int id)
        {
            await LoadAdminNotifications();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Products");
        }

        // --- DASHBOARD ---
        public async Task<IActionResult> Dashboard()
        {
            await LoadAdminNotifications();
            var now = DateTime.Now;
            var soon = now.AddHours(1);
            // Lịch hẹn sắp đến hạn hoặc quá hạn chưa có technician nhận
            var pendingAppointments = await _context.Appointments
                .Where(a => a.AppointmentTime <= soon && a.Status != "Completed" && a.Status != "Canceled")
                .Where(a => a.AppointmentVehicleDetails.All(d => d.TechnicianId == null))
                .ToListAsync();
            foreach (var appt in pendingAppointments)
            {
                if (!_context.Notifications.Any(n => n.Title.Contains("Lịch hẹn") && n.Message.Contains($"#{appt.AppointmentId}")))
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = null, // null = admin chung
                        Title = "Lịch hẹn chưa có kỹ thuật viên nhận việc",
                        Message = $"Lịch hẹn #{appt.AppointmentId} lúc {appt.AppointmentTime:dd/MM/yyyy HH:mm} chưa có kỹ thuật viên nhận.",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }
            }
            // Sản phẩm sắp hết hàng (chỉ tạo nếu tồn kho > 0 và < threshold)
            int threshold = 5;
            var lowStockProducts = await _context.Products.Where(p => p.StockQuantity > 0 && p.StockQuantity < threshold).ToListAsync();
            foreach (var p in lowStockProducts)
            {
                if (!_context.Notifications.Any(n => n.Title.Contains("Sản phẩm sắp hết hàng") && n.Message.Contains(p.ProductName)))
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = null,
                        Title = "Sản phẩm sắp hết hàng",
                        Message = $"Sản phẩm {p.ProductName} chỉ còn {p.StockQuantity} trong kho.",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }
            }
            // Sản phẩm hết hàng
            var outOfStockProducts = await _context.Products.Where(p => p.StockQuantity == 0).ToListAsync();
            foreach (var p in outOfStockProducts)
            {
                if (!_context.Notifications.Any(n => n.Title.Contains("Sản phẩm hết hàng") && n.Message.Contains(p.ProductName)))
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = null,
                        Title = "Sản phẩm hết hàng",
                        Message = $"Sản phẩm {p.ProductName} đã hết hàng trong kho!",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }
            }
            await _context.SaveChangesAsync();
            // Lấy thông báo chưa đọc cho admin
            var adminNotifications = await _context.Notifications
                .Where(n => n.UserId == null && (n.IsRead == false || n.IsRead == null))
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();
            ViewBag.AdminNotifications = adminNotifications;
            ViewBag.AdminNotificationCount = adminNotifications.Count;

            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfNextMonth = startOfMonth.AddMonths(1);

            var weeklySales = await _context.OrderItems
                .Where(oi => oi.Order.OrderDate >= startOfMonth
                  && oi.Order.OrderDate < startOfNextMonth
                  && oi.Order.Status == "Completed")
                .SumAsync(oi => (int?)oi.Quantity) ?? 0;

            // Số đơn đã hoàn thành trong tháng
            var completedOrders = await _context.Appointments
                .Where(a => a.AppointmentTime >= startOfMonth && a.AppointmentTime < startOfNextMonth && a.Status == "Completed")
                .CountAsync();

            var monthlyOrders = await _context.Appointments
                .Where(a => a.AppointmentTime >= startOfMonth && a.AppointmentTime < startOfNextMonth)
                .CountAsync();

            var visitorsOnline = 5 + new Random().Next(10, 50);

            ViewBag.WeeklySales = weeklySales;
            ViewBag.WeeklyOrders = monthlyOrders;
            ViewBag.CompletedOrders = completedOrders;
            ViewBag.VisitorsOnline = visitorsOnline;

            var year = DateTime.Today.Year;
            var monthlyOrderCounts = new int[12];
            for (int month = 1; month <= 12; month++)
            {
                var start = new DateTime(year, month, 1);
                var end = (month < 12) ? new DateTime(year, month + 1, 1) : new DateTime(year + 1, 1, 1);
                monthlyOrderCounts[month - 1] = await _context.Appointments
                    .Where(a => a.AppointmentTime >= start && a.AppointmentTime < end)
                    .CountAsync();
            }
            ViewBag.MonthlyOrderCounts = monthlyOrderCounts;

            var defaultAvatar = "/staradmin/images/faces/face8.jpg";
            var topTechnicians = await _context.Users
                .Where(u => u.Role == "Technician")
                .Select(u => new {
                    u.UserId,
                    u.FullName,
                    AvatarUrl = defaultAvatar,
                    TotalCompleted = _context.Appointments
                        .Where(a => a.Status == "Completed" && a.AppointmentVehicleDetails.Any(d => d.TechnicianId == u.UserId))
                        .Count(),
                    LastCompletedAt = _context.Appointments
                        .Where(a => a.Status == "Completed" && a.AppointmentVehicleDetails.Any(d => d.TechnicianId == u.UserId))
                        .OrderByDescending(a => a.AppointmentTime)
                        .Select(a => a.AppointmentTime)
                        .FirstOrDefault()
                })
                .OrderByDescending(t => t.TotalCompleted)
                .ThenByDescending(t => t.LastCompletedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.TopTechnicians = topTechnicians;
            return View();
        }

        // --- VOUCHER / EVENT MANAGEMENT ---
        [HttpGet]
        public async Task<IActionResult> Event()
        {
            var vouchers = await _context.PromoCodes.ToListAsync();
            return View("Event", vouchers);
        }

        [HttpGet]
        public IActionResult AddVoucher()
        {
            return View("AddVoucher", new PromoCode());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVoucher(PromoCode model, string discountType)
        {
            if (discountType == "percent")
            {
                model.DiscountAmount = null;
            }
            else
            {
                model.DiscountPercent = null;
            }
            _context.PromoCodes.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Event");
        }

        // Trang chat với user cho admin
        public async Task<IActionResult> Chat(int? userId)
        {
            await LoadAdminNotifications();
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
            if (currentUser == null) return Unauthorized();
            if (currentUser.Role != "Admin") return Forbid();
            var userList = await _context.Users.Where(u => u.Role != "Admin").ToListAsync();
            if (userId == null)
            {
                // Nếu không có userId, tự động chuyển sang user đầu tiên
                var firstUser = userList.FirstOrDefault();
                if (firstUser == null) return Content("Không có user nào để chat.");
                return RedirectToAction("Chat", new { userId = firstUser.UserId });
            }
            var chatUser = await _context.Users.FindAsync(userId);
            var admin = currentUser;
            var messages = await _context.Messages
                .Where(m => (m.SenderId == userId && m.ReceiverId == admin.UserId) || (m.SenderId == admin.UserId && m.ReceiverId == userId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
            ViewBag.IsAdmin = true;
            ViewBag.AdminId = admin.UserId;
            ViewBag.UserId = userId;
            ViewBag.ChatUser = chatUser;
            ViewBag.UserList = userList;
            return View("../Chat/Chat", messages);
        }

        // --- PAYMENT HISTORY MANAGEMENT ---
        public async Task<IActionResult> PaymentHistories(int? userId, string status, DateTime? dateFrom, DateTime? dateTo)
        {
            await LoadAdminNotifications();
            
            var query = _context.PaymentHistories
                .Include(p => p.User)
                .Include(p => p.Appointment)
                .AsQueryable();

            // Lọc theo User ID
            if (userId.HasValue)
            {
                query = query.Where(p => p.UserId == userId.Value);
                ViewBag.UserId = userId.Value;
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
                ViewBag.Status = status;
            }

            // Lọc theo khoảng thời gian
            if (dateFrom.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= dateFrom.Value);
                ViewBag.DateFrom = dateFrom.Value.ToString("yyyy-MM-dd");
            }

            if (dateTo.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= dateTo.Value.AddDays(1));
                ViewBag.DateTo = dateTo.Value.ToString("yyyy-MM-dd");
            }

            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(payments);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginViewModel model, bool RememberMe = false)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return View();
            }
            return View();
        }
    }
}
