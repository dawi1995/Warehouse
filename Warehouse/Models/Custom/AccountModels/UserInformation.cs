using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class UserInformation
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public int Role { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string VAT_Id { get; set; }
        public string Email { get; set; }
        public DateTime? Created_At { get; set; }
        public DateTime? Edited_At { get; set; }
    }
}