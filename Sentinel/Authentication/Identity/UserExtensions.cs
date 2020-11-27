using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    // Note - on schema update need to delete the generated constructor of the User class...
    public partial class User
    {
        public User()
        {
            //UserCategory = new HashSet<UserCategory>();
            UserRole = new HashSet<UserRole>();
            Id = Guid.NewGuid().ToString("D");
        }


        [NotMapped]
        public string[] MemberOf { get; set; }

    }
}
