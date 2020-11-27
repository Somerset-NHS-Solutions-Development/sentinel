using System;
using System.Collections.Generic;

#nullable disable

namespace Authentication.Models
{
    public partial class UserRole
    {
        public string UserId { get; set; }
        public int RoleId { get; set; }

        public virtual Role Role { get; set; }
        public virtual User User { get; set; }
    }
}
