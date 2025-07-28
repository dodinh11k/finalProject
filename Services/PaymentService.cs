using Microsoft.EntityFrameworkCore;
using test_2.Models;

namespace test_2.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly MyGarageFinalContext _context;
        private readonly IEmailService _emailService;

        public PaymentService(MyGarageFinalContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<PaymentHistory> CreatePaymentAsync(int appointmentId, int userId, decimal amount, string description)
        {
            var payment = new PaymentHistory
            {
                AppointmentId = appointmentId,
                UserId = userId,
                Amount = amount,
                Description = description,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.PaymentHistories.Add(payment);
            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<PaymentHistory> UpdatePaymentStatusAsync(string payOSOrderCode, string status, string? payOSTransactionId = null)
        {
            var payment = await _context.PaymentHistories
                .FirstOrDefaultAsync(p => p.PayOSOrderCode == payOSOrderCode);

            if (payment != null)
            {
                payment.Status = status;
                payment.PayOSTransactionId = payOSTransactionId;
                payment.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
            }

            return payment!;
        }

        public async Task<PaymentHistory?> GetPaymentByOrderCodeAsync(string payOSOrderCode)
        {
            return await _context.PaymentHistories
                .Include(p => p.Appointment)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PayOSOrderCode == payOSOrderCode);
        }

        public async Task<List<PaymentHistory>> GetUserPaymentHistoryAsync(int userId)
        {
            return await _context.PaymentHistories
                .Include(p => p.Appointment)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> SendPaymentConfirmationEmailAsync(PaymentHistory payment, string userEmail)
        {
            try
            {
                var subject = "Xác nhận thanh toán thành công - MyGarage";
                var body = $@"
                    <h2>Xác nhận thanh toán thành công</h2>
                    <p>Xin chào,</p>
                    <p>Cảm ơn bạn đã sử dụng dịch vụ của MyGarage. Thanh toán của bạn đã được xử lý thành công.</p>
                    
                    <h3>Thông tin thanh toán:</h3>
                    <ul>
                        <li><strong>Mã thanh toán:</strong> {payment.PaymentId}</li>
                        <li><strong>Số tiền:</strong> {payment.Amount:N0} VNĐ</li>
                        <li><strong>Phương thức:</strong> {payment.PaymentMethod}</li>
                        <li><strong>Thời gian:</strong> {payment.CreatedAt:dd/MM/yyyy HH:mm}</li>
                        <li><strong>Trạng thái:</strong> {payment.Status}</li>
                    </ul>
                    
                    <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi.</p>
                    <p>Trân trọng,<br>Đội ngũ MyGarage</p>";

                await _emailService.SendEmailAsync(userEmail, subject, body);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 