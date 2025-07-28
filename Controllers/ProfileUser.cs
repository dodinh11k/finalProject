using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using test_2.Models;
using System.Linq;

public class ProfileUser : Controller
{
    private readonly MyGarageFinalContext _context;

    public ProfileUser(MyGarageFinalContext context)
    {
        _context = context;
    }

    public IActionResult Profile()
    {
        var username = HttpContext.Session.GetString("Username");
        if (username == null)
        {
            return RedirectToAction("Login", "AccountLogin");
        }

        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            return NotFound("Không tìm thấy người dùng.");
        }

        return View(user);
    }
}
