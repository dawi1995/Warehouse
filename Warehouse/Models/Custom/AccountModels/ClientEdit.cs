using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class ClientEdit
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int Role { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string VAT_Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string UserSurname { get; set; }
        public string PhoneNumber { get; set; }
    }
}