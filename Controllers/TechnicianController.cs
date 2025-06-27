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

        if (string.IsNullOrEmpty(technicianIdStr) || !int.TryParse(technicianIdStr, out int technicianId))
        {
            return RedirectToAction("Login", "AccountLogin");
        }

        var appointments = await _context.Appointments
            .Where(a => a.TechnicianId == technicianId)
            .Include(a => a.User)
            .Include(a => a.Garage)
            .OrderByDescending(a => a.AppointmentTime)
            .ToListAsync();

        var appointmentIds = appointments.Select(a => a.AppointmentId).ToList();

        var details = await _context.AppointmentVehicleDetails
            .Where(d => appointmentIds.Contains(d.AppointmentId))
            .Include(d => d.Vehicle)
            .Include(d => d.Service)
            .ToListAsync();

        var viewModel = new AppointmentHistoryViewModel
        {
            Appointments = appointments,
            Details = details
        };

        return View("~/Views/Technician/Tasks.cshtml", viewModel);
    }
}
