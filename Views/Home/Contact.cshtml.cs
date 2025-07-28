using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using test_2.Models;

namespace test_2.Views.Home
{
    public class ContactModel : PageModel
    {
        [BindProperty]
        public string Name { get; set; }
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string Content { get; set; }
        private readonly MyGarageFinalContext _context;
        public ContactModel(MyGarageFinalContext context)
        {
            _context = context;
        }
        public void OnGet() { }
        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Content))
            {
                TempData["ContactMessage"] = "Vui lòng nhập đầy đủ thông tin.";
                return Page();
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
            return RedirectToPage();
        }
    }
}
