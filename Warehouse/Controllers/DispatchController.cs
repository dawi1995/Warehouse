using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Warehouse.Helpers;
using Warehouse.Managers;
using Warehouse.Models.Custom;
using Warehouse.Models.DAL;
using static Warehouse.Enums;

namespace Warehouse.Controllers
{
    [RoutePrefix("api/Dispatch")]
    public class DispatchController : ApiController
    {
        private readonly WarehouseEntities _context;
        public DispatchController()
        {
            _context = new WarehouseEntities();
        }

        [HttpGet]
        [Route("GetDispatches")]
        public DispatchListNumber GetDispatches(int offset = 0, int limit = int.MaxValue, string needle = "")
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin}))
            {
                try
                {
                    DispatchListNumber result = new DispatchListNumber();
                    List<DispatchList> listOfDispatches = new List<DispatchList>();
                    List<Dispatch> listOfDispatchFromDB = _context.Dispatches.Where(d=>d.Deleted_At == null && (d.Car_Id.Contains(needle) || d.Receiver_Name.Contains(needle) || d.Carrier_Name.Contains(needle))).OrderByDescending(o => o.Creation_Date).Skip(offset).Take(limit).ToList();
                    foreach (var dispatch in listOfDispatchFromDB)
                    {
                        DispatchList dispatchToResult = new DispatchList();
                        dispatchToResult.Carrier_Name = dispatch.Carrier_Name;
                        dispatchToResult.Car_Id = dispatch.Car_Id;
                        dispatchToResult.Creation_Date = dispatch.Creation_Date == null ? string.Empty : ((DateTime)dispatch.Creation_Date).ToString("dd-MM-yyyy");
                        dispatchToResult.Id = dispatch.Id;
                        dispatchToResult.Receiver_Name = dispatch.Receiver_Name;
                        listOfDispatches.Add(dispatchToResult);
                    }
                    result.ListOfDispatches = listOfDispatches;
                    result.NumberOfDispatches = DispatchManager.CountOfDispatches();
                    return result;
                } 
                catch (Exception ex)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
                }
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
            }
        }

        [HttpGet]
        [Route("GetOrderDispatch")]
        public List<DispatchDetails> GetOrderDispatch(int orderId)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin, (int)UserType.Client }))
            { 
                try
                {
                    List<DispatchDetails> result = new List<DispatchDetails>();
                    List<Orders_Positions> listOfOrdersPositions = _context.Orders_Positions.Where(o => o.Order_id == orderId && o.Deleted_At == null).ToList();
                    Delivery delivery = _context.Deliveries.FirstOrDefault(d => d.Order_Id == orderId && d.Deleted_At == null);
                    List<Deliveries_Dispatches> listOfDeliveryDispatches = _context.Deliveries_Dispatches.Where(d => d.Delivery_Id == delivery.Id && d.Deleted_At == null).ToList();
                    foreach (var deliveryDispatch in listOfDeliveryDispatches)
                    {
                        Dispatch dispatch = _context.Dispatches.FirstOrDefault(d => d.Id == deliveryDispatch.Dispatch_Id && d.Deleted_At == null);
                        DispatchDetails dispatchDetails = new DispatchDetails();
                        CarrierDispatch carrierDispatch = new CarrierDispatch();
                        ReceiverDispatch receiverDispatch = new ReceiverDispatch();
                        List<OrderPositionsDispatchInfo> listOfOrderPositionsDispatchInfo = new List<OrderPositionsDispatchInfo>();
                        List<Dispatches_Positions> listOfDispatchPositions = new List<Dispatches_Positions>();
                        foreach (var item in listOfOrdersPositions)
                        {
                            var dispatchPosition = _context.Dispatches_Positions.FirstOrDefault(d => d.Order_Position_Id == item.Id && d.Dispatch_Id == dispatch.Id && d.Deleted_At == null);
                            var dispatchesPositionsForOrderPosition = _context.Dispatches_Positions.Where(d => d.Order_Position_Id == item.Id && d.Created_At < dispatchPosition.Created_At && d.Deleted_At == null).OrderBy(d => d.Created_At).ToList();
                            int? dispatchedAmount = dispatchesPositionsForOrderPosition.Sum(d => d.Amount);
                            decimal? dispatchedWeight = dispatchesPositionsForOrderPosition.Sum(d => d.Weight_Gross);
                            OrderPositionsDispatchInfo orderPositionsDispatchInfo = new OrderPositionsDispatchInfo();
                            orderPositionsDispatchInfo.Id = item.Id;
                            orderPositionsDispatchInfo.Name = item.Name;
                            orderPositionsDispatchInfo.Amount = item.Amount;
                            orderPositionsDispatchInfo.Amount_Received = item.Amount_Received;
                            orderPositionsDispatchInfo.Amount_Before_Dispatch = item.Amount_Received - dispatchedAmount;
                            orderPositionsDispatchInfo.Amount_Dispatch = dispatchPosition.Amount;
                            orderPositionsDispatchInfo.Amount_After_Dispatch = orderPositionsDispatchInfo.Amount_Before_Dispatch - orderPositionsDispatchInfo.Amount_Dispatch;
                            orderPositionsDispatchInfo.Weight_Gross = item.Weight_Gross;
                            orderPositionsDispatchInfo.Weight_Gross_Received = item.Weight_Gross_Received;
                            orderPositionsDispatchInfo.Weight_Before_Dispatch = item.Weight_Gross_Received - dispatchedWeight;
                            orderPositionsDispatchInfo.Weight_Dispatch = dispatchPosition.Weight_Gross;
                            orderPositionsDispatchInfo.Weight_After_Dispatch = orderPositionsDispatchInfo.Weight_Before_Dispatch - orderPositionsDispatchInfo.Weight_Dispatch;
                            listOfOrderPositionsDispatchInfo.Add(orderPositionsDispatchInfo);
                        }
                        carrierDispatch.Carrier_Name = dispatch.Carrier_Name;
                        carrierDispatch.Carrier_Email = dispatch.Carrier_Email;
                        carrierDispatch.Carrier_Address = dispatch.Carrier_Address;
                        carrierDispatch.Carrier_VAT_Id = dispatch.Carrier_VAT_Id;
                        receiverDispatch.Receiver_Name = dispatch.Receiver_Name;
                        receiverDispatch.Receiver_Email = dispatch.Receiver_Email;
                        receiverDispatch.Receiver_Address = dispatch.Receiver_Address;
                        receiverDispatch.Receiver_VAT_Id = dispatch.Receiver_VAT_Id;
                        dispatchDetails.Id = dispatch.Id;
                        dispatchDetails.Dispatch_Number = dispatch.Dispatch_Number;
                        dispatchDetails.Creation_Date = dispatch.Creation_Date == null ? string.Empty : ((DateTime)(dispatch.Creation_Date)).ToString("dd-MM-yyyy");
                        dispatchDetails.Car_Id = dispatch.Car_Id;
                        dispatchDetails.Carrier = carrierDispatch;
                        dispatchDetails.Receiver = receiverDispatch;
                        dispatchDetails.ListOfOrderPositions = listOfOrderPositionsDispatchInfo;
                        result.Add(dispatchDetails);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
                }
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
            }
        }
    }
}
