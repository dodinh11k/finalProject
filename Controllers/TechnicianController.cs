using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using test_2.Models;

[Route("Technician")]
public class TechnicianController : Controller
{
    private readonly MyGarageFinalContext _context;

    public TechnicianController(MyGarageFinalContext context)
    {
        _context = context;
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
