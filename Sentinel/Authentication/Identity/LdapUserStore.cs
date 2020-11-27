using Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sentinel.Identity
{
    public class LdapUserStore : IUserRoleStore<User>
    {
        private readonly ApplicationContext _db;

        public LdapUserStore(ApplicationContext db)
        {
            _db = db;
        }

#pragma warning disable CS1998
        public async Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            // This logic moved to LdapService.CreateUserFromAttributes()
            // Check if user record created, if not create it
            //var existingUser = await _db.User.Where(w => w.UserName == user.UserName).SingleOrDefaultAsync();
            //if (existingUser == null)
            //{
            //    await CreateAsync(user, CancellationToken.None);
            //}
            //else
            //{
            //    // Need to add user roles???
            //}

            return user.Id.ToString();
        }
#pragma warning restore CS1998

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            user.UserName = normalizedName;
            return Task.FromResult(0);
        }

        public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            _db.User.Add(user);
            _db.SaveChangesAsync(cancellationToken);

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            _db.Entry(user).State = EntityState.Modified;
            _db.SaveChangesAsync(cancellationToken);

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            _db.User.Remove(user);
            _db.SaveChangesAsync(cancellationToken);

            return Task.FromResult(IdentityResult.Success);
        }

        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _db.User.FindAsync(userId);
        }

        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _db.User.Where(w => w.UserName == normalizedUserName).FirstOrDefaultAsync();
        }

        public Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            Role role = _db.Role.Where(w => w.Name == roleName).FirstOrDefault();
            if (role != null)
            {
                var userRole = new UserRole { UserId = user.Id, RoleId = role.Id };
                _db.UserRole.Add(userRole);
                _db.SaveChangesAsync();
            }

            return Task.FromResult(0);
        }

        public Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            var userRole = _db.UserRole.Where(w => w.UserId == user.Id && w.Role.Name == roleName).FirstOrDefault();
            if (userRole != null)
            {
                _db.UserRole.Remove(userRole);
                _db.SaveChangesAsync();
            }

            return Task.FromResult(0);
        }

        public Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            IList<string> roleNames = user.UserRole.Select(s => s.Role.Name).ToList();

            return Task.FromResult(roleNames);
        }

        public Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            var userInRole = _db.UserRole.Any(a => a.UserId == user.Id && a.Role.Name == roleName);

            return Task.FromResult(userInRole);
        }

        public Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            IList<User> users = _db.UserRole.Where(w => w.Role.Name == roleName).Select(s => s.User).ToList();

            return Task.FromResult(users);
        }

        public void Dispose()
        {
        }
    }
}
