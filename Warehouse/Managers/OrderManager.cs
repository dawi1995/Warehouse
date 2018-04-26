using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Warehouse.Helpers;
using Warehouse.Models.DAL;
using static Warehouse.Enums;

namespace Warehouse.Managers
{
    public class OrderManager
    {
        private static readonly WarehouseEntities _context = new WarehouseEntities();
        public static int CountOfOrders(string needle = "")
        {
            int currentUserId = UserHelper.GetCurrentUserId();
            if (UserHelper.GetCurrentUserRole() == (int)UserType.SuperAdmin || UserHelper.GetCurrentUserRole() == (int)UserType.Admin)
            {
                return _context.Orders.Where(o => o.Deleted_At == null && (o.ATB.Contains(needle) || o.Name.Contains(needle) || o.Container_Id.Contains(needle))).Count();
            }
            else
            {
                return _context.Orders.Where(o => o.Deleted_At == null && o.Creator_Id == currentUserId && (o.ATB.Contains(needle) || o.Name.Contains(needle) || o.Container_Id.Contains(needle))).Count();
            }

        }
    }
}