using Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sentinel.Identity
{
    public class LdapUserManager : Microsoft.AspNetCore.Identity.UserManager<User>
    {
        private readonly ILdapService _ldapService;

        public LdapUserManager(
    ILdapService ldapService,
    IUserStore<User> store,
    IOptions<IdentityOptions> optionsAccessor,
    IPasswordHasher<User> passwordHasher,
    IEnumerable<IUserValidator<User>> userValidators,
    IEnumerable<IPasswordValidator<User>> passwordValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    IServiceProvider services,
    ILogger<LdapUserManager> logger)
    : base(
        store,
        optionsAccessor,
        passwordHasher,
        userValidators,
        passwordValidators,
        keyNormalizer,
        errors,
        services,
        logger)
        {
            this._ldapService = ldapService;
        }
#pragma warning disable CS1998
        public override async Task<bool> CheckPasswordAsync(User user, string password)
        {
            return this._ldapService.Authenticate(user.UserName, password);
        }
#pragma warning restore CS1998

        public override Task<User> FindByNameAsync(string userName)
        {
            return Task.FromResult(this._ldapService.GetUserByUserName(userName));
        }

        public override Task<User> GetUserAsync(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            // DON'T want the id which is a guid, want AD user name
            //var id = GetUserId(principal);
            var id = principal.Identity.Name;

            return id == null ? Task.FromResult<User>(null) : FindByNameAsync(id);
        }

        public override async Task<IdentityResult> CreateAsync(User user, string password)
        {
            return await Task.FromResult(IdentityResult.Success);
        }

#pragma warning disable CS1998
        public override async Task<IList<string>> GetRolesAsync(User user)
        {
            IList<string> roles = user.UserRole.Select(s => s.Role.Name).ToList();

            return roles;
        }
#pragma warning restore CS1998

        public override async Task<bool> IsInRoleAsync(User user, string role)
        {
            var userRoleStore = Store as IUserRoleStore<User>;

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return await userRoleStore.IsInRoleAsync(user, NormalizeName(role), CancellationToken);
        }

        public override Task<IList<User>> GetUsersInRoleAsync(string roleName)
        {
            var userRoleStore = Store as IUserRoleStore<User>;

            if (roleName == null)
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            return userRoleStore.GetUsersInRoleAsync(NormalizeName(roleName), CancellationToken);
        }


        public override async Task<IdentityResult> AddToRoleAsync(User user, string role)
        {
            ThrowIfDisposed();
            var userRoleStore = Store as IUserRoleStore<User>;
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var normalizedRole = NormalizeName(role);
            if (await userRoleStore.IsInRoleAsync(user, normalizedRole, CancellationToken))
            {
                return await UserAlreadyInRoleError(user, role);
            }
            await userRoleStore.AddToRoleAsync(user, normalizedRole, CancellationToken);
            return await UpdateUserAsync(user);
        }

        public override async Task<IdentityResult> AddToRolesAsync(User user, IEnumerable<string> roles)
        {
            ThrowIfDisposed();
            var userRoleStore = Store as IUserRoleStore<User>;
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (roles == null)
            {
                throw new ArgumentNullException(nameof(roles));
            }

            foreach (var role in roles.Distinct())
            {
                var normalizedRole = NormalizeName(role);
                if (await userRoleStore.IsInRoleAsync(user, normalizedRole, CancellationToken))
                {
                    return await UserAlreadyInRoleError(user, role);
                }
                await userRoleStore.AddToRoleAsync(user, normalizedRole, CancellationToken);
            }
            return await UpdateUserAsync(user);
        }

        public override async Task<IdentityResult> RemoveFromRoleAsync(User user, string role)
        {
            ThrowIfDisposed();
            var userRoleStore = Store as IUserRoleStore<User>;
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var normalizedRole = NormalizeName(role);
            if (!await userRoleStore.IsInRoleAsync(user, normalizedRole, CancellationToken))
            {
                return await UserNotInRoleError(user, role);
            }
            await userRoleStore.RemoveFromRoleAsync(user, normalizedRole, CancellationToken);
            return await UpdateUserAsync(user);
        }

        private async Task<IdentityResult> UserAlreadyInRoleError(User user, string role)
        {
            Logger.LogWarning(5, "User {userId} is already in role {role}.", await GetUserIdAsync(user), role);
            return IdentityResult.Failed(ErrorDescriber.UserAlreadyInRole(role));
        }

        private async Task<IdentityResult> UserNotInRoleError(User user, string role)
        {
            Logger.LogWarning(6, "User {userId} is not in role {role}.", await GetUserIdAsync(user), role);
            return IdentityResult.Failed(ErrorDescriber.UserNotInRole(role));
        }

        public override async Task<IdentityResult> RemoveFromRolesAsync(User user, IEnumerable<string> roles)
        {
            ThrowIfDisposed();
            var userRoleStore = Store as IUserRoleStore<User>;
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (roles == null)
            {
                throw new ArgumentNullException(nameof(roles));
            }

            foreach (var role in roles)
            {
                var normalizedRole = NormalizeName(role);
                if (!await userRoleStore.IsInRoleAsync(user, normalizedRole, CancellationToken))
                {
                    return await UserNotInRoleError(user, role);
                }
                await userRoleStore.RemoveFromRoleAsync(user, normalizedRole, CancellationToken);
            }
            return await UpdateUserAsync(user);
        }
    }

}