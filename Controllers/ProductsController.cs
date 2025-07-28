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
            // Đếm tổng lượt đánh giá cho sản phẩm này
            var reviewCount = await _context.Reviews.CountAsync(r => r.GarageId == id);
            ViewBag.ReviewCount = reviewCount;
            return View(product);
        }
    }
}
