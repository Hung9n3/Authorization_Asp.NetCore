using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Test_Auth.Models.Users
{
    public class Claims
    {
        [Key]
        public int id { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public Claims()
        {

        }
    }
}
