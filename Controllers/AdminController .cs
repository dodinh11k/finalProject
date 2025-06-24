using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using test_2.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

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
            return View(users.ToList());
        }

        public IActionResult CreateUser() => View();
        [HttpPost]
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
        public async Task<IActionResult> EditUser(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Users");
            }
            return View(user);
        }
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Users");
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