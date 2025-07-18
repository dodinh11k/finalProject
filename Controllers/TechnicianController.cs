// Updated TechnicianController.cs: Remove direct use of Appointment.Technician / TechnicianId

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

        var technician = await _context.Users.FirstOrDefaultAsync(u => u.UserId == technicianId);
        ViewData["username"] = technician?.Username ?? "";
        ViewData["fullname"] = technician?.FullName ?? "";

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var pendingCount = await _context.Appointments.CountAsync(a => a.Status == "Pending");
        var inProgressCount = await _context.Appointments.CountAsync(a => a.Status == "In_Progress");
        var completedCount = await _context.Appointments.CountAsync(a => a.Status == "Completed");
        var todayCount = await _context.Appointments.CountAsync(a => a.AppointmentTime >= today && a.AppointmentTime < tomorrow);

        var recentAppointments = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Garage)
            .Include(a => a.AppointmentVehicleDetails)
                .ThenInclude(avd => avd.Service)
            .ToListAsync();

        ViewData["PendingCount"] = pendingCount;
        ViewData["InProgressCount"] = inProgressCount;
        ViewData["CompletedCount"] = completedCount;
        ViewData["TodayCount"] = todayCount;
        ViewData["RecentAppointments"] = recentAppointments.OrderByDescending(a => a.AppointmentTime).Take(5).ToList();

        return View("~/Views/Technician/Dashboard.cshtml");
    }

    [HttpGet("Tasks")]
    public async Task<IActionResult> Tasks()
    {
        var technicianIdStr = HttpContext.Session.GetString("UserId");
        int.TryParse(technicianIdStr, out int technicianId);

        var appointmentIds = await _context.AppointmentVehicleDetails
            .Where(d => d.TechnicianId == technicianId)
            .Select(d => d.AppointmentId)
            .Distinct()
            .ToListAsync();

        var allAppointments = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Garage)
            .Where(a => appointmentIds.Contains(a.AppointmentId))
            .OrderByDescending(a => a.AppointmentTime)
            .ToListAsync();

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

        ViewBag.MyAppointments = allAppointments;

        return View("~/Views/Technician/Tasks.cshtml", viewModel);
    }

    [HttpPost("AcceptJob")]
    public async Task<IActionResult> AcceptJob(int AppointmentId)
    {
        var technicianIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(technicianIdStr, out int technicianId))
            return RedirectToAction("Login", "AccountLogin");

        var details = await _context.AppointmentVehicleDetails
            .Where(d => d.AppointmentId == AppointmentId && d.TechnicianId == null)
            .ToListAsync();

        if (!details.Any()) return NotFound();

        foreach (var d in details)
        {
            d.TechnicianId = technicianId;
        }

        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == AppointmentId);
        if (appointment != null && (appointment.Status == "Pending" || appointment.Status == "WaitingForAccept"))
        {
            appointment.Status = "In_Progress";
        }

        await _context.SaveChangesAsync();
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
            .Include(a => a.AppointmentVehicleDetails)
                .ThenInclude(avd => avd.Service)
            .FirstOrDefaultAsync(a => a.AppointmentId == AppointmentId);

        if (appointment == null) return NotFound();

        var technicianAssigned = appointment.AppointmentVehicleDetails.Any(d => d.TechnicianId == technicianId);
        if (!technicianAssigned) return Forbid();

        appointment.Status = Status;
        await _context.SaveChangesAsync();

        if (Status == "Completed" && appointment.User != null && !string.IsNullOrEmpty(appointment.User.Email))
        {
            var toEmail = appointment.User.Email;
            var userName = appointment.User.FullName ?? appointment.User.Username;
            var appointmentId = appointment.AppointmentId.ToString();
            var appointmentTime = appointment.AppointmentTime?.ToString("dd/MM/yyyy HH:mm") ?? "";
            var serviceNames = string.Join(", ", appointment.AppointmentVehicleDetails.Select(d => d.Service?.ServiceName).Where(n => !string.IsNullOrEmpty(n)));
            var garageAddress = appointment.Garage?.Address ?? "";
            var technicianName = appointment.AppointmentVehicleDetails.FirstOrDefault(d => d.TechnicianId == technicianId)?.Technician?.FullName ?? "";

            var subject = "Thông báo hoàn thành lịch hẹn";
            var body = $"<html><body><h1>Lịch hẹn #{appointmentId} đã hoàn thành</h1><p>Kỹ thuật viên: {technicianName}</p><p>Dịch vụ: {serviceNames}</p></body></html>";

            await _emailService.SendEmailAsync(toEmail, subject, body);
        }

        return RedirectToAction("Tasks", new { tab = "mine" });
    }

    [HttpPost("Unassign")]
    public async Task<IActionResult> Unassign(int AppointmentId)
    {
        var technicianIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(technicianIdStr, out int technicianId))
            return RedirectToAction("Login", "AccountLogin");

        var details = await _context.AppointmentVehicleDetails
            .Where(d => d.AppointmentId == AppointmentId && d.TechnicianId == technicianId)
            .ToListAsync();

        if (!details.Any()) return Forbid();

        foreach (var d in details)
        {
            d.TechnicianId = null;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Tasks", new { tab = "mine" });
    }
}
