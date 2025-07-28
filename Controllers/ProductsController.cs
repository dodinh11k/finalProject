using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test_2.Models;

namespace test_2.Controllers
{
    public class ProductsController : Controller
    {
        private readonly MyGarageFinalContext _context;

        public ProductsController(MyGarageFinalContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            // Sản phẩm không có review, nên không đếm review ở đây
            ViewBag.ReviewCount = 0;
            return View(product);
        }
    }
}
