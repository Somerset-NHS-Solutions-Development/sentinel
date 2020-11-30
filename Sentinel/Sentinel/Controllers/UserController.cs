using Authentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sentinel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly ApplicationContext _db;

        public UserController(ILogger<UserController> logger, ApplicationContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var vm = await _db.User.ToListAsync();

            return View(vm);
        }

        public async Task<IActionResult> AddUser(string username, string email)
        {
            // Check if this user already exists
            var existingUser = await _db.User.SingleOrDefaultAsync(s => s.UserName == username);

            if (existingUser == null)
            {
                User newUser = new User { UserName = username, NormalizedEmail = email };
                var userRole = await _db.Role.SingleOrDefaultAsync(s => s.Name == "User");
                await _db.User.AddAsync(newUser);
                await _db.SaveChangesAsync();
                newUser.UserRole.Add(new UserRole { UserId = newUser.Id, RoleId = userRole.Id });
                await _db.SaveChangesAsync();
                _logger.LogInformation($"New user {username} added by {User.Identity.Name}");
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _db.User.Where(w => w.Id == id).FirstOrDefaultAsync();
            if (user == null) return BadRequest();

            _db.User.Remove(user);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
