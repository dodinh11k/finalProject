using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using test_2.Models;
using System;

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

        // GET: /Account/Favorites
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

        // POST: /Account/AddFavorite
        [HttpPost("AddFavorite")]
        public IActionResult AddFavorite(int productId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "AccountLogin");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "AccountLogin");

            bool exists = _context.FavoriteProducts.Any(f => f.UserId == user.UserId && f.ProductId == productId);

            if (!exists)
            {
                var fav = new FavoriteProduct
                {
                    UserId = user.UserId,
                    ProductId = productId,
                    CreatedAt = DateTime.Now
                };

                _context.FavoriteProducts.Add(fav);
                _context.SaveChanges();
                TempData["success"] = "Đã thêm vào yêu thích.";
            }
            else
            {
                TempData["info"] = "Sản phẩm đã có trong danh sách yêu thích.";
            }

            return RedirectToAction("Index", "Products");
        }

        // POST: /Account/RemoveFavorite
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
                TempData["success"] = "Đã xóa khỏi danh sách yêu thích.";
            }

            return RedirectToAction("Favorites");
        }
    }
}