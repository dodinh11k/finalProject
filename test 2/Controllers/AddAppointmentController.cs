using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using test_2.Models;

public class AddAppointmentController : Controller
{
    private readonly MyGarageFinalContext _context;

    public AddAppointmentController(MyGarageFinalContext context)
    {
        _context = context;
    }

    // GET: AddAppointment/Create
    public async Task<IActionResult> Create()
    {
        var model = new AppointmentViewModel();
        await LoadDropdowns(model);
        return View("~/Views/Appointment/Create.cshtml", model);
    }

    // POST: AddAppointment/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentViewModel model)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Phone))
        {
            ModelState.AddModelError("Phone", "Số điện thoại là bắt buộc.");
            await LoadDropdowns(model);
            return View("~/Views/Appointment/Create.cshtml", model);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == model.Phone);
        if (user == null)
        {
            user = new User
            {
                FullName = model.CustomerName,
                Phone = model.Phone,
                Username = model.Phone,
                PasswordHash = "",
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.UserId == user.UserId);
        if (vehicle == null)
        {
            vehicle = new Vehicle
            {
                UserId = user.UserId,
                Make = "Chưa xác định",
                Model = "Chưa xác định",
                LicensePlate = "Chưa rõ",
                Year = DateTime.Now.Year,
                Notes = "Xe mặc định tạo khi đặt lịch"
            };
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
        }

        var technician = await _context.Users
            .Where(u => u.Role == "Technician")
            .OrderBy(u => _context.Appointments.Count(a => a.TechnicianId == u.UserId && a.Status == "Pending"))
            .FirstOrDefaultAsync();

        if (technician == null)
        {
            ModelState.AddModelError("", "Không có kỹ thuật viên khả dụng.");
            await LoadDropdowns(model);
            return View("~/Views/Appointment/Create.cshtml", model);
        }

        var appointment = new Appointment
        {
            UserId = user.UserId,
            VehicleId = vehicle.VehicleId,
            ServiceId = model.ServiceId,
            GarageId = model.GarageId,
            TechnicianId = technician.UserId,
            AppointmentTime = model.AppointmentTime,
            Notes = model.Notes,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "AddAppointment", new { id = appointment.AppointmentId });
    }

    private async Task LoadDropdowns(AppointmentViewModel model)
    {
        model.ServiceList = await _context.Services
            .Select(s => new SelectListItem
            {
                Value = s.ServiceId.ToString(),
                Text = s.ServiceName
            }).ToListAsync();

        model.GarageList = await _context.Garages
            .Select(g => new SelectListItem
            {
                Value = g.GarageId.ToString(),
                Text = g.Address // hoặc g.Address nếu bạn muốn hiển thị địa chỉ
            }).ToListAsync();
    }

    // GET: AddAppointment/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Vehicle)
            .Include(a => a.Service)
            .Include(a => a.Garage)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);

        if (appointment == null)
        {
            return NotFound();
        }

        return View("~/Views/Appointment/Details.cshtml", appointment);
    }
}
