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
                if (delivery != null)
                {
                    if (!result.Contains(delivery.Id))
                    {
                        result.Add(delivery.Id);
                    }
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
        public static DispatchDetailsPDF GetDispatchDetails(int dispatchId)
        {
            DispatchDetailsPDF result = new DispatchDetailsPDF();
            List<OrderPositionsDispatchInfoPDF> listOfOrderPositionsDispatchInfoPDF = new List<OrderPositionsDispatchInfoPDF>();
            Dispatch dispatch = _context.Dispatches.FirstOrDefault(d => d.Id == dispatchId && d.Deleted_At == null);
            CMR_Dispatches cmrDispatch = _context.CMR_Dispatches.FirstOrDefault(c => c.Dispatch_Id == dispatchId && c.Deleted_At == null);
            List<Dispatches_Positions> dispatchPositions = _context.Dispatches_Positions.Where(d => d.Dispatch_Id == dispatchId && d.Deleted_At == null).ToList();
            
            foreach (var item in dispatchPositions)
            {
                Orders_Positions orderPosition = _context.Orders_Positions.FirstOrDefault(o => o.Id == item.Order_Position_Id && o.Deleted_At==null);
                Order order = _context.Orders.FirstOrDefault(o => o.Id == orderPosition.Order_id && o.Deleted_At == null);
                OrderPositionsDispatchInfoPDF toAddToList = new OrderPositionsDispatchInfoPDF();
                int? amountReceived = orderPosition.Amount_Received;
                decimal? weightReceived = orderPosition.Weight_Gross_Received;
                DateTime dateDispatch = dispatch.Created_At.Value.AddMilliseconds(-1);//bo znak mniejszośc działa jak <=
                List<Dispatches_Positions> listOfdispatchesPositionsForOrderPosition = _context.Dispatches_Positions.Where(d=>d.Order_Position_Id == orderPosition.Id && d.Deleted_At == null && d.Created_At.Value < dateDispatch).ToList();
                int? amountBeforeDispatch = amountReceived - listOfdispatchesPositionsForOrderPosition.Sum(d => d.Amount);
                decimal? weightBeforeDispatch = weightReceived - listOfdispatchesPositionsForOrderPosition.Sum(d => d.Weight_Gross);
                int? amountDispatch = item.Amount;
                decimal? weightDispatch = item.Weight_Gross;
                toAddToList.Id = item.Id;
                toAddToList.ATB = order.ATB;
                toAddToList.Name = orderPosition.Name;
                toAddToList.Amount_Received = amountReceived;
                toAddToList.Weight_Gross_Received = weightReceived;
                toAddToList.Amount_Before_Dispatch = amountBeforeDispatch;
                toAddToList.Weight_Before_Dispatch = weightBeforeDispatch;
                toAddToList.Amount_Dispatch = amountDispatch;
                toAddToList.Weight_Dispatch = weightDispatch;
                listOfOrderPositionsDispatchInfoPDF.Add(toAddToList);
            }

            CarrierDispatch carrierDispatch = new CarrierDispatch();
            carrierDispatch.Carrier_Address = dispatch.Carrier_Address;
            carrierDispatch.Carrier_Email = dispatch.Carrier_Email;
            carrierDispatch.Carrier_Name = dispatch.Carrier_Name;
            carrierDispatch.Carrier_VAT_Id = dispatch.Carrier_VAT_Id;

            ReceiverDispatch receiverDispatch = new ReceiverDispatch();
            receiverDispatch.Receiver_Address = dispatch.Receiver_Address;
            receiverDispatch.Receiver_Email = dispatch.Receiver_Email;
            receiverDispatch.Receiver_Name = dispatch.Receiver_Name;
            receiverDispatch.Receiver_VAT_Id = dispatch.Receiver_VAT_Id;

            SenderDispatch senderDispatch = new SenderDispatch();
            if (cmrDispatch != null)
            {
                senderDispatch.Sender_Address = cmrDispatch.Sender_Address;
                senderDispatch.Sender_Email = cmrDispatch.Sender_Email;
                senderDispatch.Sender_Name = cmrDispatch.Sender_Name;
                senderDispatch.Sender_VAT_Id = cmrDispatch.Sender_VAT_Id;

            }

            result.Id = dispatch.Id;
            result.Dispatch_Number = dispatch.Dispatch_Number;
            result.Creation_Date = dispatch.Creation_Date == null ? string.Empty : dispatch.Creation_Date.Value.ToString("dd-MM-yyyy");
            result.Car_Id = dispatch.Car_Id;
            result.Destination = cmrDispatch == null ? string.Empty : cmrDispatch.Destination;
            result.Carrier = carrierDispatch;
            result.Receiver = receiverDispatch;
            result.Sender = senderDispatch;
            result.Duty_Doc_Id = dispatch.Duty_Doc_Id;
            result.ListOfOrderPositions = listOfOrderPositionsDispatchInfoPDF;

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