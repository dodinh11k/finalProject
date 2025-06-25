using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // --- USER MANAGEMENT ---
        public async Task<IActionResult> Users(string role = null)
        {
            var users = _context.Users.AsQueryable();
            if (!string.IsNullOrEmpty(role))
                users = users.Where(u => u.Role == role);
            return View(await users.ToListAsync());
        }

        public IActionResult CreateUser()
        {
            return View(new User { IsActive = true, Username = "", PasswordHash = "" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User user)
        {
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
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(User user)
        {
            if (!ModelState.IsValid)
            {
                // Gỡ ModelState["IsActive"] để tránh giữ giá trị cũ gây sai lệch khi render lại view
                ModelState.Remove("IsActive");
                return View(user);
            }

            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);
            if (userInDb == null) return NotFound();

            // Cập nhật các trường
            userInDb.Username = user.Username;

            if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                userInDb.PasswordHash = user.PasswordHash; // Cập nhật mật khẩu nếu có nhập
            }

            userInDb.FullName = user.FullName;
            userInDb.Email = user.Email;
            userInDb.Phone = user.Phone;
            userInDb.Address = user.Address;
            userInDb.Role = user.Role;
            userInDb.IsActive = user.IsActive;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Users");
            }
            catch (DbUpdateException ex)
            {
                // Gỡ ModelState["IsActive"] để Razor render lại chính xác checkbox
                ModelState.Remove("IsActive");
                ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);

                // Cập nhật lại IsActive để đảm bảo checkbox phản ánh đúng dữ liệu mới
                user.IsActive = userInDb.IsActive;

                return View(user);
            }
        }

        // --- SERVICE MANAGEMENT ---
        public async Task<IActionResult> Services()
        {
            var services = await _context.Services.ToListAsync();
            return View(services);
        }
        public IActionResult CreateService() => View();
        [HttpPost]
        public async Task<IActionResult> CreateService(Service service)
        {
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
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();
            return View(service);
        }
        [HttpPost]
        public async Task<IActionResult> EditService(Service service)
        {
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
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction("Services");
        }

        // --- APPOINTMENT (ORDER) MANAGEMENT ---
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Service)
                .Include(a => a.Garage)
                .Include(a => a.Technician)
                .ToListAsync();
            return View(orders);
        }
        public async Task<IActionResult> EditOrder(int id)
        {
            var order = await _context.Appointments.FindAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }
        [HttpPost]
        public async Task<IActionResult> EditOrder(Appointment order)
        {
            if (ModelState.IsValid)
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
                return RedirectToAction("Orders");
            }
            return View(order);
        }
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Appointments.FindAsync(id);
            if (order == null) return NotFound();
            _context.Appointments.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("Orders");
        }

        // --- PRODUCT MANAGEMENT ---
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        public IActionResult CreateProduct()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product)
        {
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
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product product)
        {
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
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Products");
        }

        // --- DASHBOARD ---
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginViewModel model, bool RememberMe = false)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Implementation of login logic
                return View();
            }
            return View();
        }
    }
}