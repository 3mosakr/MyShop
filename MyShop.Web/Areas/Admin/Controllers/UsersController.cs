using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.DataAccess;
using MyShop.Utilities;
using System.Security.Claims;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            // display all users except the admin with open the application
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string userId = claim.Value;

            var users = _context.ApplicationUsers.Where(u => u.Id != userId).ToList();
            return View(users);
        }

        public IActionResult LockUnlock(string id)
        {
            if (id != null)
            {
                var user = _context.ApplicationUsers.FirstOrDefault(u => u.Id == id);
                if (user == null)
                {
                    return NotFound();
                }
                if (user.LockoutEnd == null || user.LockoutEnd < DateTime.Now)
                {
                    user.LockoutEnd = DateTime.Now.AddDays(15);
                }
                else
                {
                    user.LockoutEnd = DateTime.Now;
                }
                _context.SaveChanges();
            }
            
            return RedirectToAction("Index", "Users", new {area = "Admin"});
        }
    }
}
