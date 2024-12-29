using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyShop.Entities.Models;
using MyShop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        
        private readonly ApplicationDbContext _context;

        public DbInitializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async void Initialize()
        {
            // Migration
            try
            {
                if (_context.Database.GetPendingMigrations().Count() > 0)
                {
                    _context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            // Roles
            // add program roles if not exist (for one time)
            if (!_roleManager.RoleExistsAsync(SD.AdminRole).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.AdminRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.EditorRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.CustomerRole)).GetAwaiter().GetResult();
            
                // User

            
                var result = _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@myshop.com",
                    Email = "admin@myshop.com",
                    Name = "Administrator",
                    //PhoneNumber = "1234567890",
                    //Address = "Address",
                    //City = "City",

                }, "P@$$w0rd").GetAwaiter().GetResult();

                // for debug
                //if (result.Succeeded)
                //{
                //    Console.WriteLine("\n\n\n\n\nSucceeded\n\n\n\n\n");
                //}
                //else
                //{
                //    Console.WriteLine(result.ToString());
                //    Console.WriteLine("\n\n\n\n\nnot Succeeded\n\n\n\n\n");
                //}
            
                ApplicationUser user = _context.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@myshop.com");
                if (user != null)
                {
                    _userManager.AddToRoleAsync(user, SD.AdminRole).GetAwaiter().GetResult();
                }
                else // for debug
                    Console.WriteLine("\n\n\n\n\nno user created\n\n\n\n\n");
            }
        }
    }
}
