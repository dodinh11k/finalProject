using test_2.Models;

namespace test_2.Services
{
    public interface IPaymentService
    {
        Task<PaymentHistory> CreatePaymentAsync(int appointmentId, int userId, decimal amount, string description);
        Task<PaymentHistory> UpdatePaymentStatusAsync(string payOSOrderCode, string status, string? payOSTransactionId = null);
        Task<PaymentHistory?> GetPaymentByOrderCodeAsync(string payOSOrderCode);
        Task<List<PaymentHistory>> GetUserPaymentHistoryAsync(int userId);
        Task<bool> SendPaymentConfirmationEmailAsync(PaymentHistory payment, string userEmail);
    }
} 