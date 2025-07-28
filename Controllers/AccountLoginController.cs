using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using test_2.Models;
using test_2.Services;

namespace test_2.Controllers
{
    [Route("Account")]
    public class AccountLoginController : Controller
    {
        private readonly MyGarageFinalContext _context;
        private readonly IEmailService _emailService;

        public AccountLoginController(MyGarageFinalContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet("Login")]
        public IActionResult Login(string? returnUrl = null)
        {
            var role = HttpContext.Session.GetString("Role");
            if (!string.IsNullOrEmpty(role))
            {
                if (role == "Admin")
                    return RedirectToAction("Dashboard", "Admin");
                else if (role == "Technician")
                    return RedirectToAction("Dashboard", "Technician");
                else
                    return RedirectToAction("Index", "Home");
            }

            var rememberedUsername = Request.Cookies["RememberUsername"];
            ViewBag.RememberedUsername = rememberedUsername;
            ViewBag.ReturnUrl = returnUrl;

            return View("~/Views/Account/Login.cshtml");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(string username, string password, bool RememberMe = false, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đủ thông tin.";
                return View("~/Views/Account/Login.cshtml");
            }

            var user = _context.Users.FirstOrDefault(u =>
                u.Username == username &&
                u.PasswordHash == password &&
                u.IsActive == true);

            if (user == null)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu.";
                return View("~/Views/Account/Login.cshtml");
            }

            // ✅ Ghi session
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName ?? "");
            HttpContext.Session.SetString("Role", user.Role ?? "User");
            HttpContext.Session.SetString("Phone", user.Phone ?? "");

            Console.WriteLine($"✅ Session saved - UserId: {user.UserId}, Username: {user.Username}");

            if (RememberMe)
            {
                CookieOptions option = new CookieOptions { Expires = DateTime.Now.AddDays(7) };
                Response.Cookies.Append("RememberUsername", user.Username, option);
            }

            // Xác thực bằng cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()) // Thêm UserId vào claim
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (user.Role?.ToLower() == "admin")
                return RedirectToAction("Dashboard", "Admin");

            if (user.Role?.ToLower() == "technician")
                return RedirectToAction("Dashboard", "Technician");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();

            if (Request.Cookies["RememberUsername"] != null)
                Response.Cookies.Delete("RememberUsername");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Account");
        }

        [HttpGet("LoginByGoogle")]
        public async Task LoginByGoogle()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse", "Account"),
                Items = { { "prompt", "select_account" } }
            };

            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, properties);
        }

        [HttpGet("GoogleResponse")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded || result.Principal == null)
            {
                TempData["error"] = "Đăng nhập Google thất bại.";
                return RedirectToAction("Login", "Account");
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.Identity?.Name;

            if (string.IsNullOrEmpty(email))
            {
                TempData["error"] = "Không lấy được email từ Google.";
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email || u.Username == email);

            if (user == null)
            {
                user = new User
                {
                    Username = email,
                    Email = email,
                    FullName = name ?? "Người dùng Google",
                    Role = "User",
                    IsActive = true,
                    PasswordHash = "",
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // ✅ Ghi session
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName ?? "");
            HttpContext.Session.SetString("Role", user.Role ?? "User");
            HttpContext.Session.SetString("Phone", user.Phone ?? "");

            Console.WriteLine($"✅ Google Session saved - UserId: {user.UserId}, Username: {user.Username}");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()) // Thêm UserId vào claim
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (user.Role?.ToLower() == "admin")
                return RedirectToAction("Dashboard", "Admin");

            if (user.Role?.ToLower() == "technician")
                return RedirectToAction("Dashboard", "Technician");

            TempData["success"] = "Đăng nhập Google thành công!";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            return View("~/Views/Account/ForgotPassword.cshtml");
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Vui lòng nhập email.";
                return View("~/Views/Account/ForgotPassword.cshtml");
            }

            // Kiểm tra email có tồn tại trong hệ thống không
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.IsActive == true);
            if (user == null)
            {
                ViewBag.Error = "Email không tồn tại trong hệ thống.";
                return View("~/Views/Account/ForgotPassword.cshtml");
            }

            try
            {
                // Tạo mã đặt lại mật khẩu (6 chữ số ngẫu nhiên)
                var resetCode = new Random().Next(100000, 999999).ToString();
                Console.WriteLine($"🎲 Mã reset được tạo: {resetCode} cho user: {user.UserId}");

                // Lưu mã vào session hoặc database (tạm thời dùng session)
                var sessionKey = $"ResetCode_{user.UserId}";
                var emailKey = $"ResetEmail_{user.UserId}";

                HttpContext.Session.SetString(sessionKey, resetCode);
                HttpContext.Session.SetString(emailKey, email);

                Console.WriteLine($"💾 Đã lưu mã vào session với key: {sessionKey}");
                Console.WriteLine($"📧 Đã lưu email vào session với key: {emailKey}");

                // Gửi email đặt lại mật khẩu
                var subject = "Đặt lại mật khẩu - MyGarage";
                var body = GeneratePasswordResetEmail(user.FullName ?? user.Username, resetCode);

                await _emailService.SendEmailAsync(email, subject, body);

                ViewBag.Success = "Mã đặt lại mật khẩu đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra khi gửi email. Vui lòng thử lại sau.";
                // Log lỗi để debug
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
            }

            return View("~/Views/Account/ForgotPassword.cshtml");
        }

        [HttpGet("ResetPassword")]
        public IActionResult ResetPassword()
        {
            return View("~/Views/Account/ResetPassword.cshtml");
        }

        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword(string resetCode, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(resetCode) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View("~/Views/Account/ResetPassword.cshtml");
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu mới không khớp.";
                return View("~/Views/Account/ResetPassword.cshtml");
            }

            // Tìm user dựa trên resetCode trong session
            var userId = FindUserIdByResetCode(resetCode);
            if (userId == null)
            {
                ViewBag.Error = "Mã đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                return View("~/Views/Account/ResetPassword.cshtml");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản.";
                return View("~/Views/Account/ResetPassword.cshtml");
            }

            // Cập nhật mật khẩu mới
            user.PasswordHash = newPassword;
            _context.SaveChanges();

            // Xóa session reset
            HttpContext.Session.Remove($"ResetCode_{userId}");
            HttpContext.Session.Remove($"ResetEmail_{userId}");

            ViewBag.Success = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập với mật khẩu mới.";
            return View("~/Views/Account/ResetPassword.cshtml");
        }

        private int? FindUserIdByResetCode(string resetCode)
        {
            // Debug: In ra tất cả session keys để kiểm tra
            Console.WriteLine($"🔍 Tìm mã reset: {resetCode}");
            Console.WriteLine($"📋 Tất cả session keys: {string.Join(", ", HttpContext.Session.Keys)}");

            // Tìm userId dựa trên resetCode trong session
            foreach (var key in HttpContext.Session.Keys)
            {
                Console.WriteLine($"🔑 Kiểm tra key: {key}");
                if (key.StartsWith("ResetCode_"))
                {
                    var userIdStr = key.Replace("ResetCode_", "");
                    Console.WriteLine($"👤 UserId string: {userIdStr}");

                    if (int.TryParse(userIdStr, out int userId))
                    {
                        var storedCode = HttpContext.Session.GetString(key);
                        Console.WriteLine($"💾 Mã đã lưu: {storedCode}, Mã nhập: {resetCode}");

                        if (storedCode == resetCode)
                        {
                            Console.WriteLine($"✅ Tìm thấy userId: {userId}");
                            return userId;
                        }
                    }
                }
            }
            Console.WriteLine("❌ Không tìm thấy mã reset hợp lệ");
            return null;
        }

        private string GeneratePasswordResetEmail(string userName, string resetCode)
        {
            return @"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Đặt lại mật khẩu</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }
        .header {
            background-color: #007bff;
            color: white;
            padding: 20px;
            text-align: center;
            border-radius: 5px 5px 0 0;
        }
        .content {
            background-color: #f8f9fa;
            padding: 20px;
            border-radius: 0 0 5px 5px;
        }
        .reset-code {
            background-color: #e9ecef;
            padding: 15px;
            margin: 15px 0;
            border-radius: 5px;
            text-align: center;
            font-size: 24px;
            font-weight: bold;
            color: #007bff;
            letter-spacing: 5px;
        }
        .footer {
            text-align: center;
            margin-top: 20px;
            color: #666;
            font-size: 14px;
        }
    </style>
</head>
<body>
    <div class='header'>
        <h1>🔐 Đặt lại mật khẩu</h1>
    </div>
    
    <div class='content'>
        <p>Xin chào <strong>" + userName + @"</strong>,</p>
        
        <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
        
        <p><strong>Mã đặt lại mật khẩu của bạn là:</strong></p>
        
        <div class='reset-code'>" + resetCode + @"</div>
        
        <p><strong>Hướng dẫn sử dụng:</strong></p>
        <ol>
            <li>Copy mã 6 chữ số ở trên</li>
            <li>Quay lại trang đăng nhập</li>
            <li>Nhập mã này vào ô ""Mã đặt lại mật khẩu""</li>
            <li>Nhập mật khẩu mới</li>
        </ol>
        
        <p><strong>Lưu ý quan trọng:</strong></p>
        <ul>
            <li>Mã này chỉ có hiệu lực trong 10 phút</li>
            <li>Không chia sẻ mã này với bất kỳ ai</li>
            <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
        </ul>
        
        <p>Nếu bạn có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với chúng tôi.</p>
        
        <p>Trân trọng,<br>
        <strong>Đội ngũ MyGarage</strong></p>
    </div>
    
    <div class='footer'>
        <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        <p>© 2024 MyGarage. Tất cả quyền được bảo lưu.</p>
    </div>
</body>
</html>";
        }
    }
}
