﻿using System;
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
        public string PrefixVat_Id { get; set; }
        public string VAT_Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
        public string UserSurname { get; set; }

        public string Created_At { get; set; }
        public string Edited_At { get; set; }
    }
}