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
        public static int CountOfDeliveries(string needle = "", bool isCreatingDispatch = false, int dispatchId = 0)
        {
            if (isCreatingDispatch)
            {
                return (from deliveries in _context.Deliveries
                        join orders in _context.Orders on deliveries.Order_Id equals orders.Id into q
                        from orders in q.DefaultIfEmpty()
                        where (deliveries.Deleted_At == null
                        && (orders.ATB.Contains(needle) || orders.Container_Id.Contains(needle) || orders.Name.Contains(needle)))
                        select new { Delivery = deliveries, Order = orders }).OrderByDescending(d => d.Delivery.Date_Of_Delivery).Count();
            }
            else
            {
                if (dispatchId == 0)
                {
                    return (from deliveries in _context.Deliveries
                            join orders in _context.Orders on deliveries.Order_Id equals orders.Id into q
                            from orders in q.DefaultIfEmpty()
                            where (deliveries.Deleted_At == null
                            && (orders.ATB.Contains(needle) || orders.Container_Id.Contains(needle) || orders.Name.Contains(needle))
                            && deliveries.If_Delivery_Dispatch_Balanced == false)
                            select new { Delivery = deliveries, Order = orders }).OrderByDescending(d => d.Delivery.Date_Of_Delivery).Count();
                }
                else
                {
                    List<int> deliveryDispatchIds = _context.Deliveries_Dispatches.Where(d => d.Dispatch_Id == dispatchId && d.Deleted_At == null).Select(d => d.Delivery_Id).ToList();
                    return (from deliveries in _context.Deliveries
                                     join orders in _context.Orders on deliveries.Order_Id equals orders.Id into q
                                     from orders in q.DefaultIfEmpty()
                                     where (deliveries.Deleted_At == null
                                     && (orders.ATB.Contains(needle) || orders.Container_Id.Contains(needle) || orders.Name.Contains(needle))
                                     && (deliveries.If_Delivery_Dispatch_Balanced == false || deliveryDispatchIds.Contains(deliveries.Id)))
                                     select new { Delivery = deliveries, Order = orders }).OrderByDescending(d => d.Delivery.Date_Of_Delivery).Count();
                }

            }
        }
    }
}