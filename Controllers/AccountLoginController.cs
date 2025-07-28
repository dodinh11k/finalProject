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
using System.IO; // Added for Path and Directory

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
                Console.WriteLine($"📧 Bắt đầu tạo email template...");
                
                var body = await GeneratePasswordResetEmail(user.FullName ?? user.Username, resetCode);
                Console.WriteLine($"✅ Email template đã được tạo thành công");
                
                Console.WriteLine($"📧 Bắt đầu gửi email đến: {email}");
                await _emailService.SendEmailAsync(email, subject, body);
                Console.WriteLine($"✅ Email đã được gửi thành công");
                
                ViewBag.Success = "Mã đặt lại mật khẩu đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra khi gửi email. Vui lòng thử lại sau.";
                // Log lỗi để debug
                Console.WriteLine($"❌ Lỗi gửi email: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                
                // Log inner exception nếu có
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Inner exception: {ex.InnerException.Message}");
                }
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

        private async Task<string> GeneratePasswordResetEmail(string userName, string resetCode)
        {
            // Đọc file view trực tiếp
            var viewPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Views", "Email", "PasswordResetEmail.cshtml");
            Console.WriteLine($"🔍 Đang tìm file view tại: {viewPath}");
            
            if (!System.IO.File.Exists(viewPath))
            {
                Console.WriteLine($"❌ Không tìm thấy file view tại: {viewPath}");
                throw new FileNotFoundException($"View file not found: {viewPath}");
            }
            
            var viewContent = await System.IO.File.ReadAllTextAsync(viewPath);
            Console.WriteLine($"✅ Đã đọc file view thành công, kích thước: {viewContent.Length} ký tự");
            
            // Thay thế model bằng dữ liệu thực
            var body = viewContent
                .Replace("@Model.UserName", userName)
                .Replace("@Model.ResetCode", resetCode)
                .Replace("@model test_2.Models.PasswordResetEmailModel", ""); // Xóa model declaration
            
            Console.WriteLine($"✅ Đã thay thế model data thành công");
            return body;
        }
    }
}
