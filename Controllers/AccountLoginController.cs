using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using test_2.Models;

namespace test_2.Controllers
{
    [Route("Account")]
    public class AccountLoginController : Controller
    {
        private readonly MyGarageFinalContext _context;

        public AccountLoginController(MyGarageFinalContext context)
        {
            _context = context;
        }

        [HttpGet("Login")]
        public IActionResult Login(string? returnUrl = null)
        {
            var role = HttpContext.Session.GetString("Role");
            if (!string.IsNullOrEmpty(role))
            {
                if (role == "Admin")
                    return RedirectToAction("Dashboard", "Admin");
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

            if (RememberMe)
            {
                CookieOptions option = new CookieOptions { Expires = DateTime.Now.AddDays(7) };
                Response.Cookies.Append("RememberUsername", user.Username, option);
            }

            // Xác thực bằng cookie
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (user.Role?.ToLower() == "admin")
                return RedirectToAction("Dashboard", "Admin");

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

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (user.Role?.ToLower() == "admin")
                return RedirectToAction("Dashboard", "Admin");

            TempData["success"] = "Đăng nhập Google thành công!";
            return RedirectToAction("Index", "Home");
        }
    }
}
