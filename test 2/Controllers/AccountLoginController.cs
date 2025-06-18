using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using test_2.Models;

namespace test_2.Controllers
{
    public class AccountLoginController : Controller
    {
        private readonly MyGarageFinalContext _context;

        public AccountLoginController(MyGarageFinalContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Nếu đã đăng nhập (đã có session) thì chuyển về trang chính
            if (HttpContext.Session.GetString("Username") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Account/Login.cshtml");
        }

        [HttpPost]
        public IActionResult Login(string username, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đủ thông tin.";
                return View("~/Views/Account/Login.cshtml");
            }

            var user = _context.Users.FirstOrDefault(u =>
                u.Username == username &&
                u.PasswordHash == password && // Chưa mã hóa, nên bạn có thể thêm mã hóa sau nếu muốn
                u.IsActive == true);

            if (user == null)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu.";
                return View("~/Views/Account/Login.cshtml");
            }

            // Lưu thông tin người dùng vào Session
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName ?? "");
            HttpContext.Session.SetString("Role", user.Role ?? "Customer");

            // Quay lại trang trước đó (nếu có)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xoá toàn bộ session
            return RedirectToAction("Login", "AccountLogin");
        }
    }
}
