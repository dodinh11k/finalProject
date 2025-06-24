using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using test_2.Models;

namespace test_2.Controllers
{
    [Route("Account")]
    public class FavoritesController : Controller
    {
        private readonly MyGarageFinalContext _context;

        public FavoritesController(MyGarageFinalContext context)
        {
            _context = context;
        }

        [HttpGet("Favorites")]
        public IActionResult Favorites()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "AccountLogin");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "AccountLogin");

            var favorites = _context.FavoriteProducts
                .Where(f => f.UserId == user.UserId)
                .Include(f => f.Product)
                .ToList();

            return View("~/Views/Account/Favorites.cshtml", favorites);
        }

        [HttpPost("RemoveFavorite")]
        public IActionResult RemoveFavorite(int productId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "AccountLogin");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "AccountLogin");

            var favorite = _context.FavoriteProducts
                .FirstOrDefault(f => f.UserId == user.UserId && f.ProductId == productId);

            if (favorite != null)
            {
                _context.FavoriteProducts.Remove(favorite);
                _context.SaveChanges();
            }

            return RedirectToAction("Favorites");
        }
    }
}
