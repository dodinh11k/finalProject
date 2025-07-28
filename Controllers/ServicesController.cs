using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test_2.Models;
using test_2.Services;


namespace test_2.Controllers
{
    [Route("Services")] // Base route
    public class ServiceController : Controller
    {
        private readonly IServiceService _serviceService;

        public ServiceController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        // GET: /Services → redirect /Services/public
        [HttpGet("")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(PublicList));
        }
        // GET: /Services/Details/1

        [HttpGet("details/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var service = await _serviceService.GetByIdAsync(id);
            if (service == null) return NotFound();

            return View("Details", service); // nhớ có file Details.cshtml
        }

        // GET: /Services/public
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> PublicList()
        {
            var services = await _serviceService.GetAllAsync();
            return View("PublicList", services);
        }

        // GET: /Services/admin
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminList()
        {
            var services = await _serviceService.GetAllAsync();
            return View("AdminList", services);
        }

        // GET: /Services/create
        [HttpGet("create")]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View("Create");
        }

        // POST: /Services/create
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Service model)
        {
            if (ModelState.IsValid)
            {
                await _serviceService.CreateAsync(model);
                TempData["Success"] = "Service created successfully.";
                return RedirectToAction(nameof(AdminList));
            }
            return View("Create", model);
        }

        // GET: /Services/edit/5
        [HttpGet("edit/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _serviceService.GetByIdAsync(id);
            if (service == null) return NotFound();
            return View("Edit", service);
        }

        // POST: /Services/edit/5
        [HttpPost("edit/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Service model)
        {
            if (id != model.ServiceId) return BadRequest();

            if (ModelState.IsValid)
            {
                var result = await _serviceService.UpdateAsync(model);
                if (!result) return NotFound();
                TempData["Success"] = "Service updated successfully.";
                return RedirectToAction(nameof(AdminList));
            }
            return View("Edit", model);
        }

        // GET: /Services/delete/5
        [HttpGet("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _serviceService.GetByIdAsync(id);
            if (service == null) return NotFound();
            return View("Delete", service);
        }

        // POST: /Services/delete/5
        [HttpPost("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _serviceService.DeleteAsync(id);
            TempData["Success"] = "Service deleted.";
            return RedirectToAction(nameof(AdminList));
        }


        // GET: /Services/EditPartial/5
        [HttpGet("EditPartial/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditPartial(int id)
        {
            var service = await _serviceService.GetByIdAsync(id);
            if (service == null) return NotFound();

            return PartialView("_EditForm", service);
        }

        // POST: /Services/EditPartial
        [HttpPost("EditPartial")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditPartial(Service model)
        {
            if (!ModelState.IsValid)
                return PartialView("_EditForm", model);

            var result = await _serviceService.UpdateAsync(model);
            if (!result) return NotFound();

            return Json(new { success = true });
        }
    }
}
