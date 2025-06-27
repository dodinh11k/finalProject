using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using test_2.Models;

namespace test_2.Controllers
{
    [Route("Account")]
    public class AccountUpdateController : Controller
    {
        private readonly MyGarageFinalContext _context;

        public AccountUpdateController(MyGarageFinalContext context)
        {
            _context = context;
        }

        // [GET] /Account/Profile
        [HttpGet("Profile")]
        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "AccountLogin");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
                return RedirectToAction("Login", "AccountLogin");

            return View("~/Views/Account/Profile.cshtml", user);
        }

        // [POST] /Account/UpdateProfile
        [HttpPost("UpdateProfile")]
        public IActionResult UpdateProfile(string FullName, string Email, string Phone, string Address)
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "AccountLogin");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                user.FullName = FullName;
                user.Email = Email;
                user.Phone = Phone;
                user.Address = Address;

                _context.SaveChanges();

                // Cập nhật lại session tên nếu cần
                HttpContext.Session.SetString("FullName", user.FullName ?? "");
            }

            TempData["success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile"); // Action name = Profile
        }
        [HttpGet("ChangePassword")]
        public IActionResult ChangePassword()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "AccountLogin");

            return View("~/Views/Account/ChangePassword.cshtml");
        }

        // [POST] /Account/ChangePassword
        [HttpPost("ChangePassword")]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "AccountLogin");

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["error"] = "Vui lòng nhập đầy đủ thông tin.";
                return RedirectToAction("ChangePassword");
            }

            if (newPassword != confirmPassword)
            {
                TempData["error"] = "Mật khẩu mới không khớp.";
                return RedirectToAction("ChangePassword");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == currentPassword);
            if (user == null)
            {
                TempData["error"] = "Mật khẩu hiện tại không đúng.";
                return RedirectToAction("ChangePassword");
            }

            user.PasswordHash = newPassword;
            _context.SaveChanges();

            TempData["success"] = "Đổi mật khẩu thành công.";
            return RedirectToAction("ChangePassword");
        }
    }
}