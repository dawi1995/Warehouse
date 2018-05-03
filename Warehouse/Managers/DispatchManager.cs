using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Warehouse.Helpers;
using Warehouse.Models.Custom;
using Warehouse.Models.DAL;
using static Warehouse.Enums;

namespace Warehouse.Managers
{
    public class DispatchManager
    {
        private static readonly WarehouseEntities _context = new WarehouseEntities();
        public static int CountOfDispatches(string needle = "")
        {
            return _context.Dispatches.Where(d => d.Deleted_At == null && (d.Car_Id.Contains(needle) || d.Receiver_Name.Contains(needle) || d.Carrier_Name.Contains(needle))).Count();
        }
    }
}