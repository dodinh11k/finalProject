using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using test_2.Models;
using test_2.Services;
using System.Collections.Generic;

[Route("Appointment")]
public class AppointmentController : Controller
{
    private readonly MyGarageFinalContext _context;
    private readonly IEmailService _emailService;

    public AppointmentController(MyGarageFinalContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var model = new AppointmentViewModel
        {
            AppointmentTime = DateTime.Now
        };

        await LoadDropdowns(model);
        return View("~/Views/Appointment/Create.cshtml", model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns(model);
            return View("~/Views/Appointment/Create.cshtml", model);
        }

        if (model.AppointmentTime <= DateTime.Now)
        {
            ModelState.AddModelError("AppointmentTime", "Thời gian hẹn phải nằm trong tương lai.");
            await LoadDropdowns(model);
            return View("~/Views/Appointment/Create.cshtml", model);
        }

        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdStr, out int userId))
        {
            TempData["Error"] = "Bạn cần đăng nhập để đặt lịch.";
            return RedirectToAction("Login", "AccountLogin");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            TempData["Error"] = "Không tìm thấy tài khoản.";
            return RedirectToAction("Login", "AccountLogin");
        }

        try
        {
            var vehicle = new Vehicle
            {
                UserId = user.UserId,
                Make = model.VehicleMake ?? "Không rõ",
                Model = model.VehicleModel ?? "Không rõ",
                LicensePlate = model.LicensePlate ?? "Chưa rõ",
                Year = DateTime.Now.Year,
                Notes = "Xe được thêm từ đặt lịch"
            };
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            var appointment = new Appointment
            {
                UserId = user.UserId,
                GarageId = model.GarageId,
                AppointmentTime = model.AppointmentTime,
                Notes = model.Notes,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            if (model.ServiceIds != null && model.ServiceIds.Count > 0)
            {
                foreach (var serviceId in model.ServiceIds)
                {
                    var detail = new AppointmentVehicleDetail
                    {
                        AppointmentId = appointment.AppointmentId,
                        VehicleId = vehicle.VehicleId,
                        ServiceId = serviceId,
                        Quantity = 1,
                        Note = "Đặt lịch tự động"
                    };
                    _context.AppointmentVehicleDetails.Add(detail);
                }
                await _context.SaveChangesAsync();
            }

            var serviceNames = await _context.Services
                .Where(s => model.ServiceIds.Contains(s.ServiceId))
                .Select(s => s.ServiceName)
                .ToListAsync();
            var garage = await _context.Garages.FirstOrDefaultAsync(g => g.GarageId == model.GarageId);
            var technicianName = "Chưa phân công";

            if (!string.IsNullOrEmpty(user.Email))
            {
                try
                {
                    await _emailService.SendAppointmentConfirmationEmailAsync(
                        user.Email,
                        user.FullName ?? user.Username,
                        appointment.AppointmentId.ToString(),
                        appointment.AppointmentTime ?? DateTime.Now,
                        string.Join(", ", serviceNames),
                        garage?.Address ?? "Không xác định",
                        technicianName
                    );
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"Lỗi gửi email: {emailEx.Message}");
                }
            }

            return RedirectToAction("Details", "Appointment", new { id = appointment.AppointmentId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi hệ thống: " + ex.ToString());
            await LoadDropdowns(model);
            return View("~/Views/Appointment/Create.cshtml", model);
        }
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
                Text = g.Address
            }).ToListAsync();
    }

    private async Task LoadDropdowns(AppointmentEditViewModel model)
    {
        model.GarageList = await _context.Garages
            .Select(g => new SelectListItem
            {
                Value = g.GarageId.ToString(),
                Text = g.Address
            }).ToListAsync();
    }

    [HttpGet("Edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "AccountLogin");

        var appointment = await _context.Appointments
            .Include(a => a.AppointmentVehicleDetails).ThenInclude(d => d.Vehicle)
            .FirstOrDefaultAsync(a => a.AppointmentId == id && a.UserId == userId);

        if (appointment == null) return NotFound();

        var detail = appointment.AppointmentVehicleDetails.FirstOrDefault();
        if (detail == null) return NotFound();

        var vehicle = detail.Vehicle;

        var model = new AppointmentEditViewModel
        {
            AppointmentId = appointment.AppointmentId,
            AppointmentTime = appointment.AppointmentTime ?? DateTime.Now,
            Notes = appointment.Notes,
            GarageId = appointment.GarageId ?? 0,
            VehicleMake = vehicle?.Make,
            VehicleModel = vehicle?.Model,
            LicensePlate = vehicle?.LicensePlate
        };

        await LoadDropdowns(model);
        return View("~/Views/Appointment/Edit.cshtml", model);
    }

    [HttpPost("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AppointmentEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns(model);
            return View("~/Views/Appointment/Edit.cshtml", model);
        }

        if (model.AppointmentTime <= DateTime.Now)
        {
            ModelState.AddModelError("AppointmentTime", "Thời gian hẹn phải nằm trong tương lai.");
            await LoadDropdowns(model);
            return View("~/Views/Appointment/Edit.cshtml", model);
        }

        var appointment = await _context.Appointments
            .Include(a => a.AppointmentVehicleDetails)
            .FirstOrDefaultAsync(a => a.AppointmentId == model.AppointmentId);

        if (appointment == null) return NotFound();

        var detail = appointment.AppointmentVehicleDetails.FirstOrDefault();
        if (detail == null) return NotFound();

        var vehicle = await _context.Vehicles.FindAsync(detail.VehicleId);
        if (vehicle != null)
        {
            vehicle.Make = model.VehicleMake;
            vehicle.Model = model.VehicleModel;
            vehicle.LicensePlate = model.LicensePlate;
        }

        appointment.AppointmentTime = model.AppointmentTime;
        appointment.Notes = model.Notes;
        appointment.GarageId = model.GarageId;

        await _context.SaveChangesAsync();
        return RedirectToAction("History");
    }

    [HttpGet("Cancel/{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdStr, out int userId))
            return RedirectToAction("Login", "AccountLogin");

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentId == id && a.UserId == userId);

        if (appointment == null)
            return NotFound();

        if (appointment.Status == "Completed" || appointment.Status == "Canceled")
        {
            TempData["Error"] = "Lịch hẹn này không thể hủy.";
            return RedirectToAction("History");
        }

        appointment.Status = "Canceled";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Lịch hẹn đã được hủy.";
        return RedirectToAction("History");
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Garage)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);

        if (appointment == null)
            return NotFound();

        var vehicleDetails = await _context.AppointmentVehicleDetails
            .Include(d => d.Vehicle)
            .Include(d => d.Service)
            .Where(d => d.AppointmentId == id)
            .ToListAsync();

        var viewModel = new AppointmentDetailsViewModel
        {
            Appointment = appointment,
            VehicleDetails = vehicleDetails
        };

        return View("~/Views/Appointment/Details.cshtml", viewModel);
    }

    [HttpGet("History")]
    public async Task<IActionResult> History()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");

        if (!int.TryParse(userIdStr, out int userId))
        {
            HttpContext.Session.SetString("ReturnUrl", Url.Action("History", "Appointment"));
            return RedirectToAction("Login", "AccountLogin");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            return RedirectToAction("Login", "AccountLogin");
        }

        var appointments = await _context.Appointments
            .Where(a => a.UserId == user.UserId)
            .Include(a => a.Garage)
            .OrderByDescending(a => a.AppointmentTime)
            .ToListAsync();

        var appointmentIds = appointments.Select(a => a.AppointmentId).ToList();

        var allDetails = await _context.AppointmentVehicleDetails
            .Where(d => appointmentIds.Contains(d.AppointmentId))
            .Include(d => d.Vehicle)
            .Include(d => d.Service)
            .Include(d => d.Technician)
            .ToListAsync();

        var viewModel = new AppointmentHistoryViewModel
        {
            Appointments = appointments,
            Details = allDetails
        };

        return View("~/Views/Appointment/History.cshtml", viewModel);
    }
}
