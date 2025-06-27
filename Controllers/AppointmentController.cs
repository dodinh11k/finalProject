using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using test_2.Models;
using System.Collections.Generic;

[Route("Appointment")]
public class AppointmentController : Controller
{
    private readonly MyGarageFinalContext _context;

    public AppointmentController(MyGarageFinalContext context)
    {
        _context = context;
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var model = new AppointmentViewModel();
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

        // ✅ Lấy UserId từ Session
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
        {
            TempData["Error"] = "Bạn cần đăng nhập để đặt lịch.";
            return RedirectToAction("Login", "AccountLogin");
        }

        // ✅ Lấy thông tin người dùng
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            TempData["Error"] = "Không tìm thấy người dùng.";
            return RedirectToAction("Login", "AccountLogin");
        }

        try
        {
            // 🚗 Thêm xe mới
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

            // 👷‍♂️ Tìm kỹ thuật viên có ít lịch "Pending" nhất
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

            // 🗓️ Tạo lịch hẹn
            var appointment = new Appointment
            {
                UserId = user.UserId,
                GarageId = model.GarageId,
                TechnicianId = technician.UserId,
                AppointmentTime = model.AppointmentTime,
                Notes = model.Notes,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // 🔧 Thêm chi tiết dịch vụ
            var detail = new AppointmentVehicleDetail
            {
                AppointmentId = appointment.AppointmentId,
                VehicleId = vehicle.VehicleId,
                ServiceId = model.ServiceId,
                Quantity = 1,
                Note = "Đặt lịch tự động"
            };
            _context.AppointmentVehicleDetails.Add(detail);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = appointment.AppointmentId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
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

        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
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
            .Include(a => a.Technician)
            .OrderByDescending(a => a.AppointmentTime)
            .ToListAsync();

        var appointmentIds = appointments.Select(a => a.AppointmentId).ToList();

        var allDetails = await _context.AppointmentVehicleDetails
            .Where(d => appointmentIds.Contains(d.AppointmentId))
            .Include(d => d.Vehicle)
            .Include(d => d.Service)
            .ToListAsync();

        var viewModel = new AppointmentHistoryViewModel
        {
            Appointments = appointments,
            Details = allDetails
        };

        return View("~/Views/Appointment/History.cshtml", viewModel);
    }
}
