using Microsoft.AspNetCore.Mvc;
using test_2.Services;
using System.Threading.Tasks;

namespace test_2.Controllers
{
    [Route("EmailTest")]
    public class EmailTestController : Controller
    {
        private readonly EmailTestService _emailTestService;

        public EmailTestController(EmailTestService emailTestService)
        {
            _emailTestService = emailTestService;
        }

        [HttpGet("Test")]
        public IActionResult Test()
        {
            return View();
        }

        [HttpPost("Test")]
        public async Task<IActionResult> Test(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Vui lòng nhập email";
                return View();
            }

            try
            {
                await _emailTestService.TestEmailAsync(email);
                TempData["Success"] = "Email test đã được gửi thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi gửi email: {ex.Message}";
            }

            return View();
        }
    }
} 