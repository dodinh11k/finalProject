using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using test_2.Models;

namespace test_2.Controllers
{
    public class ServicesController : Controller
    {
        private readonly MyGarageFinalContext _context;

        public ServicesController(MyGarageFinalContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .Include(s => s.Garages)
                .Select(s => new ServiceViewModel
                {
                    ServiceId = s.ServiceId,
                    image_url = s.image_url,
                    ServiceName = s.ServiceName,
                    
                })
                .ToListAsync();

            return View("~/Views/Services/Index.cshtml", services);
        }
    }
}
