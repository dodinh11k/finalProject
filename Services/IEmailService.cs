using System.Threading.Tasks;

namespace test_2.Services
{
    public interface IEmailService
    {
        Task SendAppointmentConfirmationEmailAsync(string toEmail, string userName, string appointmentId, DateTime appointmentTime, string serviceName, string garageAddress, string technicianName);
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
} 