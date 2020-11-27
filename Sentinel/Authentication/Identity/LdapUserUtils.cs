using Authentication.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sentinel.Identity
{
    public class LdapUserUtils
    {
        public static User GetUserRecord(ClaimsPrincipal principal, ApplicationContext db)
        {
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = db.User.Find(userId);

            return user;
        }

        public static List<string> GetUserRoles(ClaimsPrincipal principal, ApplicationContext db)
        {
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = db.User.Where(w => w.Id == userId).Include(i => i.UserRole).ThenInclude(t => t.Role).SingleOrDefault();
            if (user == null) return new List<string>();

            return user.UserRole.Select(s => s.Role.Name).ToList();
        }

        public static string GetUserId(ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}
