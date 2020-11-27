using System;
using System.Collections.Generic;

#nullable disable

namespace Authentication.Models
{
    public partial class User
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedEmail { get; set; }
        public string SecurityStamp { get; set; }
        public string ConcurrencyStamp { get; set; }

        public virtual ICollection<UserRole> UserRole { get; set; }
    }
}
