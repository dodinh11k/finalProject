using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace test_2.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAppointmentConfirmationEmailAsync(string toEmail, string userName, string appointmentId, DateTime appointmentTime, string serviceName, string garageAddress, string technicianName)
        {
            var subject = "Xác nhận đặt lịch hẹn thành công";
            var body = GenerateAppointmentConfirmationEmail(userName, appointmentId, appointmentTime, serviceName, garageAddress, technicianName);
            
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration["Email:From"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = body;
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _configuration["Email:SmtpServer"],
                int.Parse(_configuration["Email:Port"]),
                SecureSocketOptions.StartTls
            );
            
            await smtp.AuthenticateAsync(
                _configuration["Email:Username"],
                _configuration["Email:Password"]
            );
            
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        private string GenerateAppointmentConfirmationEmail(string userName, string appointmentId, DateTime appointmentTime, string serviceName, string garageAddress, string technicianName)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Xác nhận đặt lịch hẹn</title>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            line-height: 1.6;
                            color: #333;
                            max-width: 600px;
                            margin: 0 auto;
                            padding: 20px;
                        }}
                        .header {{
                            background-color: #007bff;
                            color: white;
                            padding: 20px;
                            text-align: center;
                            border-radius: 5px 5px 0 0;
                        }}
                        .content {{
                            background-color: #f8f9fa;
                            padding: 20px;
                            border-radius: 0 0 5px 5px;
                        }}
                        .appointment-details {{
                            background-color: white;
                            padding: 15px;
                            margin: 15px 0;
                            border-radius: 5px;
                            border-left: 4px solid #007bff;
                        }}
                        .detail-row {{
                            margin: 10px 0;
                        }}
                        .label {{
                            font-weight: bold;
                            color: #007bff;
                        }}
                        .footer {{
                            text-align: center;
                            margin-top: 20px;
                            color: #666;
                            font-size: 14px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>🎉 Đặt lịch hẹn thành công!</h1>
                    </div>
                    
                    <div class='content'>
                        <p>Xin chào <strong>{userName}</strong>,</p>
                        
                        <p>Cảm ơn bạn đã đặt lịch hẹn với chúng tôi. Dưới đây là thông tin chi tiết về lịch hẹn của bạn:</p>
                        
                        <div class='appointment-details'>
                            <div class='detail-row'>
                                <span class='label'>Mã lịch hẹn:</span> #{appointmentId}
                            </div>
                            <div class='detail-row'>
                                <span class='label'>Thời gian:</span> {appointmentTime:dd/MM/yyyy HH:mm}
                            </div>
                            <div class='detail-row'>
                                <span class='label'>Dịch vụ:</span> {serviceName}
                            </div>
                            <div class='detail-row'>
                                <span class='label'>Địa chỉ garage:</span> {garageAddress}
                            </div>
                            <div class='detail-row'>
                                <span class='label'>Kỹ thuật viên:</span> {technicianName}
                            </div>
                        </div>
                        
                        <p><strong>Lưu ý quan trọng:</strong></p>
                        <ul>
                            <li>Vui lòng đến đúng giờ hẹn</li>
                            <li>Mang theo giấy tờ xe và thông tin cá nhân</li>
                            <li>Nếu có thay đổi, vui lòng liên hệ chúng tôi sớm nhất</li>
                        </ul>
                        
                        <p>Nếu bạn có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với chúng tôi.</p>
                        
                        <p>Trân trọng,<br>
                        <strong>Đội ngũ MyGarage</strong></p>
                    </div>
                    
                    <div class='footer'>
                        <p>Email này được gửi tự động, vui lòng không trả lời.</p>
                        <p>© 2024 MyGarage. Tất cả quyền được bảo lưu.</p>
                    </div>
                </body>
                </html>";
        }
    }
} 