using Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sentinel.Identity
{
    public class LdapUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User>
    {
        public LdapUserClaimsPrincipalFactory(
            LdapUserManager userManager,
            IOptions<IdentityOptions> optionsAccessor)
                : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Add a claim for each AD group that this user is a member of
            if (user.MemberOf != null)
            {
                foreach (var group in user.MemberOf)
                {
                    identity.AddClaim(new Claim(group, ""));
                }
            }

            // As we're using custom classes we must also add the roles in manually
            foreach (Role r in user.UserRole.Select(s => s.Role))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, r.Name, ""));
            }

            return identity;
        }
    }
}
