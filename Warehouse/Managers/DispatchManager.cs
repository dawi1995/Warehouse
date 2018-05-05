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

        public static List<int> GetListOfDeliveriesIdsForDispatch(Dispatch dispatchFromDB)
        {
            List<int> result = new List<int>();
            List<Deliveries_Dispatches> listOfDeliveryDispatches = _context.Deliveries_Dispatches.Where(d => d.Dispatch_Id == dispatchFromDB.Id).ToList();
            foreach (var item in listOfDeliveryDispatches)
            {
                Delivery delivery = _context.Deliveries.FirstOrDefault(d => d.Id == item.Delivery_Id);
                if (!result.Contains(delivery.Id))
                {
                    result.Add(delivery.Id);
                }
            }
            return result;
        }
        public static List<int> GetIdstoRemove(List<EditDispatchPositions> dispatchPositionsFromUser, List<Dispatches_Positions> dispatchPositionsFromDB)
        {
            List<int> result = new List<int>();
            List<int> dispatchpositionsFromUserIds = new List<int>();
            List<int> dispatchpositionsFromDBIds = new List<int>();
            foreach (var item in dispatchPositionsFromUser)
            {
                if (item.Id != null)
                {
                    dispatchpositionsFromUserIds.Add((int)item.Id);
                }
            }
            foreach (var item in dispatchPositionsFromDB)
            {
                dispatchpositionsFromDBIds.Add(item.Id);
            }
            foreach (var id in dispatchpositionsFromDBIds)
            {
                if (!dispatchpositionsFromUserIds.Contains(id))
                {
                    result.Add(id);
                }

            }
            return result;
        }
        //public static List<int> GetListOfOrderPositionsIds(List<Orders_Positions> listOfOrderPositions)
        //{
        //    List<int> result = new List<int>();
        //    foreach (var item in listOfOrderPositions)
        //    {
        //        if (!result.Contains(item.Id))
        //        {
        //            result.Add(item.Id);
        //        }
        //    }
        //    return result;
        //}
    }
}