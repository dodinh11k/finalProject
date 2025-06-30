using System.Threading.Tasks;

namespace test_2.Services
{
    public class EmailTestService
    {
        private readonly IEmailService _emailService;

        public EmailTestService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task TestEmailAsync(string testEmail)
        {
            try
            {
                await _emailService.SendAppointmentConfirmationEmailAsync(
                    testEmail,
                    "Nguyễn Văn Test",
                    "TEST001",
                    DateTime.Now.AddDays(1),
                    "Bảo dưỡng xe",
                    "123 Đường ABC, Quận 1, TP.HCM",
                    "Kỹ thuật viên A"
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi test email: {ex.Message}", ex);
            }
        }
    }
} 