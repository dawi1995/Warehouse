using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Warehouse.Models.DAL;

namespace Warehouse.Managers
{
    public class DeliveryManager
    {
        private static readonly WarehouseEntities _context = new WarehouseEntities();
        public static int CountOfDeliveries(string needle = "")
        {
            return (from deliveries in _context.Deliveries
                join orders in _context.Orders on deliveries.Order_Id equals orders.Id into q
                from orders in q.DefaultIfEmpty()
                where (deliveries.Deleted_At == null && (orders.ATB.Contains(needle) || orders.Container_Id.Contains(needle) || orders.Name.Contains(needle)))
                select new { Delivery = deliveries, Order = orders }).Count();
        }
    }
}