using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using test_2.Models;

namespace test_2.Controllers
{
    public class AddAppointmentController : Controller
    {
        private readonly MyGarageFinalContext _context;

        public AddAppointmentController(MyGarageFinalContext context)
        {
            _context = context;
        }

        // GET: AddAppointment/Create
        public IActionResult Create()
        {
            LoadDropdownData();
            return View("~/Views/Appointment/Create.cshtml");
        }

        // POST: AddAppointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentViewModel model)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Phone))
            {
                ModelState.AddModelError("Phone", "Số điện thoại là bắt buộc.");
                LoadDropdownData();
                return View("~/Views/Appointment/Create.cshtml", model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == model.Phone);
            if (user == null)
            {
                user = new User
                {
                    FullName = model.CustomerName,
                    Phone = model.Phone,
                    Address = "", // Địa chỉ bỏ qua
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var technician = await _context.Users
                .Where(u => u.Role == "Technician")
                .OrderBy(u => _context.Appointments.Count(a => a.TechnicianId == u.UserId && a.Status == "Pending"))
                .FirstOrDefaultAsync();

            if (technician == null)
            {
                ModelState.AddModelError("", "Không có kỹ thuật viên khả dụng.");
                LoadDropdownData();
                return View("~/Views/Appointment/Create.cshtml", model);
            }

            var appointment = new Appointment
            {
                UserId = user.UserId,
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

            return RedirectToAction("Details", "Appointment", new { id = appointment.AppointmentId });
        }

        private void LoadDropdownData()
        {
            ViewBag.Services = _context.Services.ToList();
            ViewBag.Garages = _context.Garages.ToList();
        }
    }
}
