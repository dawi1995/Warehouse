using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class UserRegistration
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public int Role { get; set; }
    }
}