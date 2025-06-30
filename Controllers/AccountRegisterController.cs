using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.RegularExpressions;
using test_2.Models;

namespace test_2.Controllers
{
    public class AccountRegisterController : Controller
    {
        private readonly MyGarageFinalContext _context;

        public AccountRegisterController(MyGarageFinalContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View("~/Views/Account/Register.cshtml");
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            // Bắt buộc kiểm tra trước
            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/Register.cshtml", model);
            }

            // 1. Kiểm tra username đã tồn tại
            if (_context.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
            }

            // 2. Kiểm tra mật khẩu có ít nhất 1 ký tự đặc biệt và 1 chữ cái
            if (!Regex.IsMatch(model.Password, @"^(?=.*[A-Za-z])(?=.*[^A-Za-z0-9]).{6,}$"))
            {
                ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 1 chữ cái và 1 ký tự đặc biệt.");
            }

            // 3. Kiểm tra email hợp lệ thủ công (nếu muốn custom thêm)
            if (!string.IsNullOrEmpty(model.Email) &&
                !Regex.IsMatch(model.Email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ.");
            }

            // 4. Kiểm tra số điện thoại Việt Nam
            if (!string.IsNullOrEmpty(model.Phone) &&
                !Regex.IsMatch(model.Phone, @"^(0|\+84)(3[2-9]|5[6|8|9]|7[06-9]|8[1-9]|9[0-9])[0-9]{7}$"))
            {
                ModelState.AddModelError("Phone", "Số điện thoại không hợp lệ.");
            }

            // 5. Kiểm tra địa chỉ không được rỗng
            if (string.IsNullOrWhiteSpace(model.Address))
            {
                ModelState.AddModelError("Address", "Địa chỉ không được bỏ trống.");
            }

            // Nếu có bất kỳ lỗi nào => return lại View
            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/Register.cshtml", model);
            }

            // 6. Tạo user và lưu
            var user = new User
            {
                Username = model.Username,
                PasswordHash = model.Password, // Có thể hash nếu muốn
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                IsActive = true,
                Role = "User"
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["RegisterSuccess"] = "true";
            return View("~/Views/Account/Register.cshtml");
        }
    }
}
