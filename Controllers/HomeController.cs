using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using test_2.Models;

namespace test_2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MyGarageFinalContext _context;

        public HomeController(ILogger<HomeController> logger, MyGarageFinalContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(string Name, string Email, string Content)
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Content))
            {
                TempData["ContactMessage"] = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            var noti = new Notification
            {
                UserId = null, // gửi cho admin chung
                Title = $"Phản hồi từ khách hàng: {Name}",
                Message = $"Email: {Email}\nNội dung: {Content}",
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(noti);
            _context.SaveChanges();
            TempData["ContactMessage"] = "Phản hồi của bạn đã được gửi tới admin!";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Contact()
        {
            return View();
        }
    }
}
