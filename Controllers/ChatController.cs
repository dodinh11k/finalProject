using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using test_2.Models;

namespace test_2.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly MyGarageFinalContext _context;
        public ChatController(MyGarageFinalContext context)
        {
            _context = context;
        }

        // Lấy lịch sử chat giữa user và admin
        public async Task<IActionResult> History(int? userId)
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
            if (currentUser == null) return Unauthorized();

            // Nếu là admin, cho phép chọn userId để xem lịch sử
            if (currentUser.Role == "Admin")
            {
                if (userId == null) return View("AdminSelectUser", await _context.Users.Where(u => u.Role != "Admin").ToListAsync());
                ViewBag.ChatUser = await _context.Users.FindAsync(userId);
            }
            else
            {
                userId = currentUser.UserId;
                ViewBag.ChatUser = null;
            }

            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
            if (admin == null) return Content("No admin found");

            var messages = await _context.Messages
                .Where(m => (m.SenderId == userId && m.ReceiverId == admin.UserId) || (m.SenderId == admin.UserId && m.ReceiverId == userId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
            ViewBag.IsAdmin = currentUser.Role == "Admin";
            ViewBag.AdminId = admin.UserId;
            ViewBag.UserId = userId;
            return View("Chat", messages);
        }

        [HttpGet("/api/chat/admin-id")]
        public async Task<IActionResult> GetAdminId()
        {
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
            if (admin == null) return NotFound();
            return Json(new { adminId = admin.UserId });
        }

        [HttpGet("/api/chat/history")]
        public async Task<IActionResult> GetChatHistory()
        {
            var username = User.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return Unauthorized();
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
            if (admin == null) return NotFound();
            var messages = await _context.Messages
                .Where(m => (m.SenderId == user.UserId && m.ReceiverId == admin.UserId) || (m.SenderId == admin.UserId && m.ReceiverId == user.UserId))
                .OrderBy(m => m.Timestamp)
                .Select(m => new {
                    senderId = m.SenderId,
                    content = m.Content,
                    timestamp = m.Timestamp.ToString("HH:mm dd/MM")
                })
                .ToListAsync();
            return Json(messages);
        }
    }
} 