using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace test_2.Services
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // Lấy UserId từ claim (nếu đã add vào claim khi đăng nhập)
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
} 