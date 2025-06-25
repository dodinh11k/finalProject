using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using test_2.Models;

namespace test_2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AddServicesController : Controller
    {
        private readonly MyGarageFinalContext _context;

        public AddServicesController(MyGarageFinalContext context)
        {
            _context = context;
        }

        // GET: AddServices/Create
        public async Task<IActionResult> Create()
        {
            var model = new ServiceCreateViewModel
            {
                GarageList = await _context.Garages
                    .Select(g => new SelectListItem
                    {
                        Value = g.GarageId.ToString(),
                        Text = g.Name
                    }).ToListAsync()
            };

            return View("~/Views/AddServices/Create.cshtml", model);
        }

        // POST: AddServices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.GarageList = await _context.Garages
                    .Select(g => new SelectListItem
                    {
                        Value = g.GarageId.ToString(),
                        Text = g.Name
                    }).ToListAsync();

                return View("~/Views/AddServices/Create.cshtml", model);
            }

            var service = new Service
            {
                ServiceName = model.ServiceName,
                Description = model.Description,
                Price = model.Price,
                image_url = model.image_url
            };

            if (model.SelectedGarageIds != null && model.SelectedGarageIds.Any())
            {
                var garages = await _context.Garages
                    .Where(g => model.SelectedGarageIds.Contains(g.GarageId))
                    .ToListAsync();

                service.Garages = garages;
            }

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Services");
        }
    }
}
