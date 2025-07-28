//using Microsoft.AspNetCore.Mvc;
//using System.Linq;
//using test_2.Models;

//namespace test_2.Controllers
//{
//    public class AccountRegisterController : Controller
//    {
//        private readonly MyGarageFinalContext _context;

//        public AccountRegisterController(MyGarageFinalContext context)
//        {
//            _context = context;
//        }

//        [HttpGet]
//        public IActionResult Register()
//        {
//            return View("~/Views/Account/Register.cshtml"); // Tự động tìm Views/Account/Register.cshtml
//        }

//        [HttpPost]
//        public IActionResult Register(RegisterViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
//                return View("~/Views/Account/Register.cshtml",model);
//            }

//            // Kiểm tra username đã tồn tại chưa
//            if (_context.Users.Any(u => u.Username == model.Username))
//            {
//                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
//                return View("~/Views/Account/Register.cshtml",model);
//            }

//            // Gợi ý mã hóa mật khẩu (nên dùng thư viện như BCrypt.Net)
//            // string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

//            var user = new User
//            {
//                Username = model.Username,
//                PasswordHash = model.Password, // ⚠️ Tạm thời dùng mật khẩu thô, không an toàn
//                FullName = model.FullName,
//                Email = model.Email,
//                Phone = model.Phone,
//                Address = model.Address,
//                IsActive = true,
//                Role = "Customer"
//            };

//            _context.Users.Add(user);
//            _context.SaveChanges();

//            return RedirectToAction("Login", "AccountLogin");
//        }
//    }
//}
    