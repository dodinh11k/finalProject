using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test_2.Models; // namespace đúng với context của bạn
namespace test_2.Controllers;

[Route("Garages")]
public class GarageController : Controller
{
    private readonly MyGarageFinalContext _context;

    public GarageController(MyGarageFinalContext context)
    {
        _context = context;
    }

    // GET: /Garage
    public async Task<IActionResult> Index()
    {
        var garages = await _context.Garages.ToListAsync();
        return View(garages);
    }
}
