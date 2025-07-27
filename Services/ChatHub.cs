using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using test_2.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace test_2.Services
{
    public class ChatHub : Hub
    {
        private readonly MyGarageFinalContext _context;
        public ChatHub(MyGarageFinalContext context)
        {
            _context = context;
        }
        // Gửi tin nhắn từ user tới admin hoặc ngược lại
        public async Task SendMessage(int senderId, int receiverId, string message)
        {
            Console.WriteLine($"[ChatHub] SendMessage CALLED: sender={senderId}, receiver={receiverId}, message={message}");
            try
            {
                Console.WriteLine($"[ChatHub] SendMessage: sender={senderId}, receiver={receiverId}, message={message}");
                var msg = new Message
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Content = message,
                    Timestamp = DateTime.Now
                };
                _context.Messages.Add(msg);
                await _context.SaveChangesAsync();
                await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", senderId, message);
                await Clients.User(senderId.ToString()).SendAsync("ReceiveMessage", senderId, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ChatHub] ERROR: " + ex);
                throw;
            }
        }

        // Đăng ký userId vào connection
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"[ChatHub] OnConnectedAsync: UserIdentifier={Context.UserIdentifier}");
            await base.OnConnectedAsync();
        }
    }
} 