using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using test_2.Models;

[Route("Technician")]
public class TechnicianController : Controller
{
    private readonly MyGarageFinalContext _context;
    private readonly test_2.Services.IEmailService _emailService;

    public TechnicianController(MyGarageFinalContext context, test_2.Services.IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var technicianIdStr = HttpContext.Session.GetString("UserId");
        int.TryParse(technicianIdStr, out int technicianId);

        // Lấy thông tin technician
        var technician = await _context.Users.FirstOrDefaultAsync(u => u.UserId == technicianId);

        ViewData["username"] = technician?.Username ?? "";
        ViewData["fullname"] = technician?.FullName ?? "";

        // Lấy thống kê thực tế từ database
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        // Đếm lịch hẹn đang chờ (Pending)
        var pendingCount = await _context.Appointments
            .Where(a => a.Status == "Pending")
            .CountAsync();

        // Đếm lịch hẹn đang thực hiện (In_Progress)
        var inProgressCount = await _context.Appointments
            .Where(a => a.Status == "In_Progress")
            .CountAsync();

        // Đếm lịch hẹn đã hoàn thành (Completed)
        var completedCount = await _context.Appointments
            .Where(a => a.Status == "Completed")
            .CountAsync();

        // Đếm lịch hẹn hôm nay
        var todayCount = await _context.Appointments
            .Where(a => a.AppointmentTime >= today && a.AppointmentTime < tomorrow)
            .CountAsync();

        // Lấy lịch hẹn gần đây (5 lịch hẹn mới nhất)
        var recentAppointments = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Garage)
            .Include(a => a.Technician)
            .Include(a => a.AppointmentVehicleDetails)
                .ThenInclude(avd => avd.Service)
            .OrderByDescending(a => a.AppointmentTime)
            .Take(5)
            .ToListAsync();

        // Truyền dữ liệu thống kê vào ViewData
        ViewData["PendingCount"] = pendingCount;
        ViewData["InProgressCount"] = inProgressCount;
        ViewData["CompletedCount"] = completedCount;
        ViewData["TodayCount"] = todayCount;
        ViewData["RecentAppointments"] = recentAppointments;

        return View("~/Views/Technician/Dashboard.cshtml");
    }

    [HttpGet("Tasks")]
    public async Task<IActionResult> Tasks()
    {
        var technicianIdStr = HttpContext.Session.GetString("UserId");
        int.TryParse(technicianIdStr, out int technicianId);

        // Tất cả lịch hẹn
        var allAppointments = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Garage)
            .Include(a => a.Technician)
            .OrderByDescending(a => a.AppointmentTime)
            .ToListAsync();

        // Lịch đã nhận của technician này (hiện cả khi trạng thái là Pending nếu đã được gán)
        var myAppointments = allAppointments
            .Where(a => a.TechnicianId == technicianId && a.Status != "Cancelled")
            .ToList();

        var appointmentIds = allAppointments.Select(a => a.AppointmentId).ToList();

        var details = await _context.AppointmentVehicleDetails
            .Where(d => appointmentIds.Contains(d.AppointmentId))
            .Include(d => d.Vehicle)
            .Include(d => d.Service)
            .ToListAsync();

        var viewModel = new AppointmentHistoryViewModel
        {
            Appointments = allAppointments,
            Details = details
        };

        ViewBag.MyAppointments = myAppointments;

        return View("~/Views/Technician/Tasks.cshtml", viewModel);
    }

    [HttpPost("AcceptJob")]
    public async Task<IActionResult> AcceptJob(int AppointmentId)
    {
        var technicianIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(technicianIdStr, out int technicianId))
            return RedirectToAction("Login", "AccountLogin");

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentId == AppointmentId && a.TechnicianId == technicianId);

        if (appointment == null)
            return NotFound();

        // Chỉ cho nhận nếu đang ở trạng thái Pending hoặc WaitingForAccept
        if (appointment.Status == "Pending" || appointment.Status == "WaitingForAccept")
        {
            appointment.Status = "In_Progress";
            await _context.SaveChangesAsync();
        }

        // Chuyển hướng sang tab "Các lịch bạn đã nhận"
        return RedirectToAction("Tasks", new { tab = "mine" });
    }

    [HttpPost("UpdateStatus")]
    public async Task<IActionResult> UpdateStatus(int AppointmentId, string Status)
    {
        var technicianIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(technicianIdStr, out int technicianId))
            return RedirectToAction("Login", "AccountLogin");

        var appointment = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Garage)
            .Include(a => a.Technician)
            .Include(a => a.AppointmentVehicleDetails)
                .ThenInclude(avd => avd.Service)
            .FirstOrDefaultAsync(a => a.AppointmentId == AppointmentId);

        if (appointment == null)
            return NotFound();

        // Nếu nhận việc, gán technician và chuyển trạng thái
        if (Status == "In_Progress")
        {
            appointment.TechnicianId = technicianId;
            appointment.Status = "In_Progress";
        }
        else
        {
            // Chỉ cho phép cập nhật trạng thái nếu technician là người nhận
            if (appointment.TechnicianId != technicianId)
                return Forbid();
            appointment.Status = Status;
        }

        await _context.SaveChangesAsync();

        // Gửi email khi hoàn thành
        if (Status == "Completed" && appointment.User != null && !string.IsNullOrEmpty(appointment.User.Email))
        {
            var toEmail = appointment.User.Email;
            var userName = appointment.User.FullName ?? appointment.User.Username;
            var appointmentId = appointment.AppointmentId.ToString();
            var appointmentTime = appointment.AppointmentTime?.ToString("dd/MM/yyyy HH:mm") ?? "";
            var serviceNames = string.Join(", ", appointment.AppointmentVehicleDetails.Select(d => d.Service?.ServiceName).Where(n => !string.IsNullOrEmpty(n)));
            var garageAddress = appointment.Garage?.Address ?? "";
            var technicianName = appointment.Technician?.FullName ?? appointment.Technician?.Username ?? "";
            var subject = "Thông báo hoàn thành lịch hẹn";
            var body = $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Hoàn thành lịch hẹn</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; background: #f4f6fb; margin: 0; padding: 0; }}
                        .mail-container {{ max-width: 600px; margin: 30px auto; background: #fff; border-radius: 12px; box-shadow: 0 4px 24px rgba(0,0,0,0.08); overflow: hidden; }}
                        .header {{ background: linear-gradient(90deg, #4e54c8, #8f94fb); color: #fff; padding: 32px 24px 20px 24px; text-align: center; }}
                        .header h1 {{ margin: 0; font-size: 2rem; letter-spacing: 1px; }}
                        .header .icon {{ font-size: 3rem; margin-bottom: 8px; }}
                        .content {{ padding: 32px 24px; }}
                        .content h2 {{ color: #4e54c8; margin-top: 0; }}
                        .info-list {{ list-style: none; padding: 0; margin: 24px 0; }}
                        .info-list li {{ margin-bottom: 12px; font-size: 1.05rem; }}
                        .info-label {{ font-weight: bold; color: #4e54c8; }}
                        .footer {{ background: #f4f6fb; color: #888; text-align: center; padding: 18px 10px; font-size: 0.95rem; }}
                        .thankyou {{ color: #27ae60; font-weight: bold; font-size: 1.1rem; margin-top: 18px; }}
                    </style>
                </head>
                <body>
                    <div class='mail-container'>
                        <div class='header'>
                            <div class='icon'>✅</div>
                            <h1>Lịch hẹn đã hoàn thành!</h1>
                        </div>
                        <div class='content'>
                            <h2>Xin chào {userName},</h2>
                            <p>Chúng tôi xin thông báo lịch hẹn của bạn đã được <b>hoàn thành thành công</b> bởi kỹ thuật viên của MyGarage.</p>
                            <ul class='info-list'>
                                <li><span class='info-label'>Mã lịch hẹn:</span> #{appointmentId}</li>
                                <li><span class='info-label'>Thời gian:</span> {appointmentTime}</li>
                                <li><span class='info-label'>Dịch vụ:</span> {serviceNames}</li>
                                <li><span class='info-label'>Địa chỉ garage:</span> {garageAddress}</li>
                                <li><span class='info-label'>Kỹ thuật viên:</span> {technicianName}</li>
                            </ul>
                            <div class='thankyou'>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của MyGarage!<br>Chúc bạn luôn an toàn & vững tay lái!</div>
                        </div>
                        <div class='footer'>
                            Email này được gửi tự động từ hệ thống MyGarage.<br>
                            Nếu có thắc mắc, vui lòng liên hệ đội ngũ hỗ trợ.<br>
                            &copy; 2024 MyGarage. All rights reserved.
                        </div>
                    </div>
                </body>
                </html>
            ";
            await _emailService.SendEmailAsync(toEmail, subject, body);
        }

        // Chuyển sang tab các lịch đã nhận
        return RedirectToAction("Tasks", new { tab = "mine" });
    }

    [HttpPost("Unassign")]
    public async Task<IActionResult> Unassign(int AppointmentId)
    {
        var technicianIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(technicianIdStr, out int technicianId))
            return RedirectToAction("Login", "AccountLogin");

        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == AppointmentId);

        if (appointment == null || appointment.TechnicianId != technicianId)
            return Forbid();

        appointment.TechnicianId = null;
        appointment.Status = "Pending";
        await _context.SaveChangesAsync();

        return RedirectToAction("Tasks", new { tab = "mine" });
    }
}
