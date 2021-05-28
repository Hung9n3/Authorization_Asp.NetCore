using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Test_Auth.Models.Roles;

namespace Test_Auth.Models.Users
{
    public class User : IdentityUser
    {
        public string Guid { get; set; }
        public string Password { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; } = new HashSet<UserRole>();
    }
}
