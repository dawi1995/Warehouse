using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse
{
    public class Enums
    {
        public enum UserType
        {
            Client = 1,
            Admin = 2,
            SuperAdmin = 3
        };

        public enum OrderStatus
        {
            Reported = 1,
            Accepted = 2,
            Difference = 3
        };
    }
}