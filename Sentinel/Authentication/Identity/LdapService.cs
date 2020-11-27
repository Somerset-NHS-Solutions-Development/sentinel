using Authentication.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.Identity
{
    public class LdapService : ILdapService
    {
        private readonly ApplicationContext _db;

        public LdapService(ApplicationContext db)
        {
            _db = db;
        }

        public bool Authenticate(string SamAccountName, string password)
        {
            bool authenticated = false;

            using (PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, "YDH"))
            {
                authenticated = principalContext.ValidateCredentials(SamAccountName, // + "@YDH.NHS.UK", 
                                                                        password,
                                                                        ContextOptions.Negotiate |
                                                                        ContextOptions.Signing |
                                                                        ContextOptions.Sealing);
            }

            return authenticated;
        }

        public User GetUserByUserName(string userName)
        {
            User user;

            using PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, "YDH");
            var userPrincipal = UserPrincipal.FindByIdentity(principalContext, IdentityType.SamAccountName, userName);

            user = this.CreateUserFromAttributes(userName, userPrincipal);

            return user;
        }


        private User CreateUserFromAttributes(string userName, UserPrincipal principal)
        {
            var dirEntry = principal.GetUnderlyingObject() as DirectoryEntry;

            var ldapUser = _db.User
                            .Where(w => w.UserName == userName)
                            .Include(i => i.UserRole)
                            .ThenInclude(t => t.Role)
                            .FirstOrDefault();

            if (ldapUser == null)
            {
                ldapUser = new User
                {
                    UserName = principal.SamAccountName,
                    NormalizedEmail = principal.EmailAddress
                };
                _db.User.Add(ldapUser);
                _db.SaveChanges();
            }

            // Get the AD groups
            PrincipalSearchResult<Principal> groups = principal.GetAuthorizationGroups();
            List<string> groupList = new List<string>();

            // iterate over all groups
            foreach (Principal p in groups)
            {
                // make sure to add only group principals
                if (p is GroupPrincipal)
                {
                    groupList.Add(((GroupPrincipal)p).Name);
                }
            }
            ldapUser.MemberOf = groupList.ToArray();

            return ldapUser;
        }
    }
}
