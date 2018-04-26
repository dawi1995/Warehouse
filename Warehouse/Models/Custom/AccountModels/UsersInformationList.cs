using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class UsersInformationList
    {
        public int NumberOfUsers { get; set; }
        public List<UserInformation> ListOfUsers{ get; set; }
    }
}