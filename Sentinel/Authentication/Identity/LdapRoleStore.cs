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
    public class LdapRoleStore : IRoleStore<Role>
    {
        private readonly ApplicationContext _db;

        public LdapRoleStore(ApplicationContext db)
        {
            _db = db;
        }

        public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
        {
            _db.Role.Add(role);
            await _db.SaveChangesAsync(CancellationToken.None);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
        {
            _db.Entry(role).State = EntityState.Modified;
            await _db.SaveChangesAsync(CancellationToken.None);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            _db.Role.Remove(role);
            await _db.SaveChangesAsync(CancellationToken.None);

            return IdentityResult.Success;
        }

        public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id.ToString());
        }

        public Task<string> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public async Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            _db.Entry(role).State = EntityState.Modified;
            await _db.SaveChangesAsync(CancellationToken.None);

            return;
        }

        public Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            return GetRoleNameAsync(role, cancellationToken);
        }

        public Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
        {
            return SetRoleNameAsync(role, normalizedName, cancellationToken);
        }

        public async Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            try
            {
                int roleIdAsInt = Int32.Parse(roleId);
                var role = await _db.Role.FindAsync(new object[] { roleIdAsInt }, CancellationToken.None);
                return role;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            var role = await _db.Role.Where(w => w.Name == normalizedRoleName).FirstOrDefaultAsync(CancellationToken.None);

            return role;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
