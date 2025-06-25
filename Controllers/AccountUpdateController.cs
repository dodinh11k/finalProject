//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Http;
//using System.Linq;
//using test_2.Models;

//namespace test_2.Controllers
//{
//    public class AccountLoginController : Controller
//    {
//        private readonly MyGarageFinalContext _context;

//        public AccountLoginController(MyGarageFinalContext context)
//        {
//            _context = context;
//        }

//        [HttpGet]
//        public IActionResult Login()
//        {
//            // Nếu đã đăng nhập (đã có session) thì chuyển về Home
//            if (HttpContext.Session.GetString("Username") != null)
//            {
//                return RedirectToAction("Index", "Home");
//            }
//            return View("~/Views/Account/Login.cshtml");
//        }

//        [HttpPost]
//        public IActionResult Login(string username, string password)
//        {
//            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
//            {
//                ViewBag.Error = "Vui lòng nhập đủ thông tin.";
//                return View("~/Views/Account/Login.cshtml");
//            }

//            var user = _context.Users.FirstOrDefault(u =>
//                u.Username == username &&
//                u.PasswordHash == password &&
//                u.IsActive == true);

//            if (user == null)
//            {
//                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu.";
//                return View("~/Views/Account/Login.cshtml");
//            }

//            // Lưu thông tin user vào Session
//            HttpContext.Session.SetString("Username", user.Username);
//            HttpContext.Session.SetString("FullName", user.FullName ?? "");
//            HttpContext.Session.SetString("Role", user.Role ?? "Customer");

//            return RedirectToAction("Index", "Home");
//        }

//        [HttpGet]
//        public IActionResult Logout()
//        {
//            // Xóa toàn bộ session khi logout
//            HttpContext.Session.Clear();
//            return RedirectToAction("Login", "AccountLogin");
//        }
//    }
//}
