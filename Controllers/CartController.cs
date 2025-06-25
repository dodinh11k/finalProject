using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using test_2.Models;
using Microsoft.EntityFrameworkCore;

namespace test_2.Controllers
{
    public class CartController : Controller
    {
        private readonly MyGarageFinalContext _context;
        public CartController(MyGarageFinalContext context)
        {
            _context = context;
        }

        // Hiển thị giỏ hàng
        public IActionResult Index()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "AccountLogin");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "AccountLogin");

            var cart = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.UserId == user.UserId && o.Status == "Cart");

            return View(cart);
        }

        // Thêm vào giỏ hàng
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "AccountLogin");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "AccountLogin");

            var cart = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.UserId == user.UserId && o.Status == "Cart");

            if (cart == null)
            {
                cart = new Order
                {
                    UserId = user.UserId,
                    OrderDate = DateTime.Now,
                    Status = "Cart",
                    TotalAmount = 0
                };
                _context.Orders.Add(cart);
                _context.SaveChanges();
            }

            var orderItem = cart.OrderItems.FirstOrDefault(oi => oi.ProductId == productId);
            var product = _context.Products.Find(productId);
            if (orderItem != null)
            {
                orderItem.Quantity += quantity;
            }
            else
            {
                cart.OrderItems.Add(new OrderItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product?.Price ?? 0
                });
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // Xóa khỏi giỏ hàng
        [HttpPost]
        public IActionResult Remove(int orderItemId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "AccountLogin");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return RedirectToAction("Login", "AccountLogin");

            var orderItem = _context.OrderItems.Include(oi => oi.Order)
                .FirstOrDefault(oi => oi.OrderItemId == orderItemId && oi.Order.UserId == user.UserId && oi.Order.Status == "Cart");
            if (orderItem != null)
            {
                _context.OrderItems.Remove(orderItem);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
} 