using Authentication.Models;


namespace Sentinel.Identity
{
    public interface ILdapService
    {
        bool Authenticate(string SamAccountName, string password);
        public User GetUserByUserName(string userName);

    }
}
