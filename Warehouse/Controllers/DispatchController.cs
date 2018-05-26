﻿using System;
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
        private readonly PDFManager _pdfManager;
        public DispatchController()
        {
            _context = new WarehouseEntities();
            _pdfManager = new PDFManager();
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
                    result.NumberOfDispatches = DispatchManager.CountOfDispatches(needle);
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
        [Route("GetDispatchDetails")]
        public DispatchDetailsDispatchInfo GetDispatchDetails(int dispatchId)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin, (int)UserType.Client }))
            {
                DispatchDetailsDispatchInfo result = new DispatchDetailsDispatchInfo();
                List<DispatchOrderList> listOfDispatchOrders = new List<DispatchOrderList>();

                CarrierDispatch carrier = new CarrierDispatch();
                ReceiverDispatch receiver = new ReceiverDispatch();
                CMRDispatch cmrToResult = new CMRDispatch();

                try
                {
                    Dispatch dispatchFromDB = _context.Dispatches.FirstOrDefault(d => d.Id == dispatchId && d.Deleted_At == null);
                    if (dispatchFromDB != null)
                    {
                        List<int> listOfDeliveryIdsForDispatch = DispatchManager.GetListOfDeliveriesIdsForDispatch(dispatchFromDB);
                        foreach (var deliveryId in listOfDeliveryIdsForDispatch)
                        {
                            DispatchOrderList dispatchOrderList = new DispatchOrderList();
                            Delivery delivery = _context.Deliveries.FirstOrDefault(d => d.Id == deliveryId && d.Deleted_At == null);
                            //if (delivery != null)
                            //{
                                Order order = _context.Orders.FirstOrDefault(o => o.Id == delivery.Order_Id && o.Deleted_At == null);
                            //}

                            Dispatch dispatch = _context.Dispatches.FirstOrDefault(d => d.Id == dispatchId && d.Deleted_At == null);
                            if (dispatch.CMR_Id != null)
                            {
                                int cmrId = Convert.ToInt32(dispatch.CMR_Id);
                                CMR_Dispatches cmrDispatch = _context.CMR_Dispatches.FirstOrDefault(c => c.Id == cmrId && c.Deleted_At == null);
                                cmrToResult.Commodity_Type = cmrDispatch.Commodity_Type;
                                cmrToResult.Destination = cmrDispatch.Destination;
                                cmrToResult.Sender_Address = cmrDispatch.Sender_Address;
                                cmrToResult.Sender_Email = cmrDispatch.Sender_Email;
                                cmrToResult.Sender_Name = cmrDispatch.Sender_Name;
                                cmrToResult.Sender_VAT_Id = cmrDispatch.Sender_VAT_Id;

                            }

                            List<DispatchPositionsDispatchInfo> listOfDispatchPositions = new List<DispatchPositionsDispatchInfo>();

                            List<Orders_Positions> listOfOrdersPositions = _context.Orders_Positions.Where(o => o.Order_id == order.Id && o.Deleted_At == null).ToList();
                            foreach (var orderPosition in listOfOrdersPositions)
                            {
                                List<Dispatches_Positions> dispatchPositionsFromDB = _context.Dispatches_Positions.Where(d => d.Dispatch_Id == dispatchId && d.Order_Position_Id == orderPosition.Id && d.Deleted_At == null).ToList();
                                foreach (var dispatchPositionFromDB in dispatchPositionsFromDB)
                                {
                                    DispatchPositionsDispatchInfo dispatchPosition = new DispatchPositionsDispatchInfo();
                                    dispatchPosition.Id = dispatchPositionFromDB.Id;
                                    dispatchPosition.Amount = dispatchPositionFromDB.Amount;
                                    dispatchPosition.Weight_Gross = dispatchPositionFromDB.Weight_Gross;
                                    dispatchPosition.Name = _context.Orders_Positions.FirstOrDefault(o => o.Id == dispatchPositionFromDB.Order_Position_Id && o.Deleted_At == null).Name;
                                    listOfDispatchPositions.Add(dispatchPosition);
                                }
                            }
                            dispatchOrderList.DeliveryId = delivery.Id;
                            dispatchOrderList.OrderId = order.Id;
                            dispatchOrderList.ATB = order.ATB;
                            dispatchOrderList.ListOfDispatchPositions = listOfDispatchPositions;
                            listOfDispatchOrders.Add(dispatchOrderList);

                        }

                        carrier.Carrier_Address = dispatchFromDB.Carrier_Address;
                        carrier.Carrier_Email = dispatchFromDB.Carrier_Email;
                        carrier.Carrier_VAT_Id = dispatchFromDB.Carrier_VAT_Id;
                        carrier.Carrier_Name = dispatchFromDB.Carrier_Name;
                        carrier.Carrier_PhoneNumber = dispatchFromDB.Carrier_PhoneNumber;
                        receiver.Receiver_Address = dispatchFromDB.Receiver_Address;
                        receiver.Receiver_Email = dispatchFromDB.Receiver_Email;
                        receiver.Receiver_Name = dispatchFromDB.Receiver_Name;
                        receiver.Receiver_VAT_Id = dispatchFromDB.Receiver_VAT_Id;
                        receiver.Receiver_PhoneNumber = dispatchFromDB.Receiver_PhoneNumber;
                        result.Dispatch_Number = dispatchFromDB.Dispatch_Number;
                        result.Carrier = carrier;
                        result.Id = dispatchFromDB.Id;
                        result.ListOfDispatchOrders = listOfDispatchOrders;
                        result.Receiver = receiver;
                        result.isCMR = dispatchFromDB.If_CMR;
                        result.CmrDispatch = cmrToResult;
                        result.Duty_Doc_Id = dispatchFromDB.Duty_Doc_Id;
                        result.Car_Id = dispatchFromDB.Car_Id;
                        result.Creation_Date = dispatchFromDB.Creation_Date == null ? string.Empty : dispatchFromDB.Creation_Date.Value.ToString("dd-MM-yyyy");
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



        [HttpGet]
        [Route("GetOrderDispatches")]
        public List<DispatchDetails> GetOrderDispatches(int orderId)
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
                            if (dispatchPosition != null)
                            {
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
                        }
                        carrierDispatch.Carrier_Name = dispatch.Carrier_Name;
                        carrierDispatch.Carrier_Email = dispatch.Carrier_Email;
                        carrierDispatch.Carrier_Address = dispatch.Carrier_Address;
                        carrierDispatch.Carrier_VAT_Id = dispatch.Carrier_VAT_Id;
                        carrierDispatch.Carrier_PhoneNumber = dispatch.Carrier_PhoneNumber;
                        receiverDispatch.Receiver_Name = dispatch.Receiver_Name;
                        receiverDispatch.Receiver_Email = dispatch.Receiver_Email;
                        receiverDispatch.Receiver_Address = dispatch.Receiver_Address;
                        receiverDispatch.Receiver_VAT_Id = dispatch.Receiver_VAT_Id;
                        receiverDispatch.Receiver_PhoneNumber = dispatch.Receiver_PhoneNumber;
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

        [HttpPost]
        [Route("EditDispatch")]
        public RequestResult EditDispatch([FromBody]EditDispatch editDispatch, bool isCMR)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin }))
            {
                DateTime dateOfEdit = DateTime.Now;
                RequestResult result = new RequestResult();
                try
                {
                    CMR_Dispatches CMRDispatch = _context.CMR_Dispatches.FirstOrDefault(c => c.Dispatch_Id == editDispatch.CMRId);
                    if (isCMR)
                    {
                        if (CMRDispatch != null)
                        {
                            CMRDispatch.Edited_At = dateOfEdit;
                            CMRDispatch.Destination = editDispatch.CMRDispatch.Destination;
                            CMRDispatch.Sender_Address = editDispatch.CMRDispatch.Sender_Address;
                            CMRDispatch.Sender_Email = editDispatch.CMRDispatch.Sender_Email;
                            CMRDispatch.Sender_Name = editDispatch.CMRDispatch.Sender_Name;
                            CMRDispatch.Sender_VAT_Id = editDispatch.CMRDispatch.Sender_VAT_Id;
                            _context.SaveChanges();
                        }
                        else
                        {
                            CMRDispatch = new CMR_Dispatches();
                            CMRDispatch.Created_At = dateOfEdit;
                            CMRDispatch.Destination = editDispatch.CMRDispatch.Destination;
                            CMRDispatch.Dispatch_Id = 0;
                            CMRDispatch.Sender_Address = editDispatch.CMRDispatch.Sender_Address;
                            CMRDispatch.Sender_Email = editDispatch.CMRDispatch.Sender_Email;
                            CMRDispatch.Sender_Name = editDispatch.CMRDispatch.Sender_Name;
                            CMRDispatch.Sender_VAT_Id = editDispatch.CMRDispatch.Sender_VAT_Id;
                            _context.CMR_Dispatches.Add(CMRDispatch);
                            _context.SaveChanges();
                        }
                    }
                    
                    Dispatch dispatchToEdit = _context.Dispatches.FirstOrDefault(o => o.Id == editDispatch.Id && o.Deleted_At == null);
                    if (dispatchToEdit != null)
                    {
                        dispatchToEdit.Carrier_Address = editDispatch.Carrier.Carrier_Address;
                        dispatchToEdit.Carrier_Email = editDispatch.Carrier.Carrier_Email;
                        dispatchToEdit.Carrier_Name = editDispatch.Carrier.Carrier_Name;
                        dispatchToEdit.Carrier_VAT_Id = editDispatch.Carrier.Carrier_VAT_Id;
                        dispatchToEdit.Carrier_PhoneNumber = editDispatch.Carrier.Carrier_PhoneNumber;
                        dispatchToEdit.Car_Id = editDispatch.Car_Id;
                        dispatchToEdit.Creation_Date = editDispatch.Creation_Date;
                        dispatchToEdit.Dispatch_Number = editDispatch.Dispatch_Number;
                        dispatchToEdit.Receiver_Address = editDispatch.Receiver.Receiver_Address;
                        dispatchToEdit.Receiver_Email = editDispatch.Receiver.Receiver_Email;
                        dispatchToEdit.Receiver_Name = editDispatch.Receiver.Receiver_Name;
                        dispatchToEdit.Receiver_VAT_Id = editDispatch.Receiver.Receiver_VAT_Id;
                        dispatchToEdit.Receiver_PhoneNumber = editDispatch.Receiver.Receiver_PhoneNumber;
                        dispatchToEdit.Edited_At = dateOfEdit;                
                        dispatchToEdit.If_CMR = isCMR;
                        dispatchToEdit.Duty_Doc_Id = editDispatch.Duty_Doc_Id;
                        dispatchToEdit.CMR_Id = CMRDispatch == null ? null : CMRDispatch.Id.ToString();

                        _context.SaveChanges();
                        var dispatchPositionsFromDB = _context.Dispatches_Positions.Where(d => d.Dispatch_Id == editDispatch.Id && d.Deleted_At == null).ToList();

                        //Editing and adding orderPositions
                        foreach (var dispatchPosition in editDispatch.DispatchPositions)
                        {
                           
                            foreach (var dispatchPositionFromDB in dispatchPositionsFromDB)
                            {
                                if (dispatchPosition.Id == dispatchPositionFromDB.Id)
                                {
                                    Dispatches_Positions dispatchPositionForParameter = _context.Dispatches_Positions.FirstOrDefault(d => d.Id == dispatchPosition.Id && d.Deleted_At == null);
                                    Orders_Positions orderPosition = _context.Orders_Positions.FirstOrDefault(o => o.Id == dispatchPositionForParameter.Order_Position_Id && o.Deleted_At == null);
                                    dispatchPositionFromDB.Amount = dispatchPosition.Amount;
                                    dispatchPositionFromDB.Edited_At = dateOfEdit;
                                    dispatchPositionFromDB.Weight_Gross = dispatchPosition.Amount* orderPosition.Unit_Weight;
                                    _context.SaveChanges();
                                    Delivery delivery = _context.Deliveries.FirstOrDefault(d => d.Order_Id == orderPosition.Order_id && d.Deleted_At == null);
                                    List<Orders_Positions> listOfOrdersPositionsForOrderPosition = _context.Orders_Positions.Where(o => o.Order_id == orderPosition.Order_id && o.Deleted_At==null).ToList();
                                    bool isBalanced = true;
                                    foreach (var orderPositionForOrder in listOfOrdersPositionsForOrderPosition)
                                    {
                                        List<Dispatches_Positions> dispatchPositions = _context.Dispatches_Positions.Where(d => d.Order_Position_Id == orderPositionForOrder.Id && d.Deleted_At==null).ToList();
                                        if (orderPositionForOrder.Amount_Received != dispatchPositions.Sum(d => d.Amount))
                                        {
                                            isBalanced = false;
                                        }
                                    }
                                    if (isBalanced)
                                    {
                                        delivery.If_Delivery_Dispatch_Balanced = true;
                                    }
                                    _context.SaveChanges();
                                }
                                if (dispatchPosition.Id == null)
                                {
                                    Dispatches_Positions dispatchPositionsToAdd = new Dispatches_Positions();
                                    dispatchPositionsToAdd.Created_At = dateOfEdit;
                                    dispatchPositionsToAdd.Amount = dispatchPosition.Amount;
                                    dispatchPositionsToAdd.Weight_Gross = dispatchPosition.Weight_Gross;//Do zmiany jak ustalimy sposób edycji dispatchPositions
                                    _context.Dispatches_Positions.Add(dispatchPositionsToAdd);
                                    Dispatches_Positions dispatchPositionForParameter = _context.Dispatches_Positions.FirstOrDefault(d => d.Id == dispatchPositionsToAdd.Id && d.Deleted_At == null);
                                    Orders_Positions orderPosition = _context.Orders_Positions.FirstOrDefault(o => o.Id == dispatchPositionForParameter.Order_Position_Id && o.Deleted_At == null);
                                    Delivery delivery = _context.Deliveries.FirstOrDefault(d => d.Order_Id == orderPosition.Order_id && d.Deleted_At == null);
                                    List<Orders_Positions> listOfOrdersPositionsForOrderPosition = _context.Orders_Positions.Where(o => o.Order_id == orderPosition.Order_id && o.Deleted_At==null).ToList();
                                    bool isBalanced = true;
                                    foreach (var orderPositionForOrder in listOfOrdersPositionsForOrderPosition)
                                    {
                                        List<Dispatches_Positions> dispatchPositions = _context.Dispatches_Positions.Where(d => d.Order_Position_Id == orderPositionForOrder.Id && d.Deleted_At==null).ToList();
                                        if (orderPositionForOrder.Amount_Received != dispatchPositions.Sum(d => d.Amount))
                                        {
                                            isBalanced = false;
                                        }
                                    }
                                    if (isBalanced)
                                    {
                                        delivery.If_Delivery_Dispatch_Balanced = true;
                                    }
                                    _context.SaveChanges();
                                    if (_context.Deliveries_Dispatches.FirstOrDefault(d => d.Delivery_Id == delivery.Id && d.Dispatch_Id == editDispatch.Id && d.Deleted_At == null) == null)
                                    {
                                        Deliveries_Dispatches deliveryDispatch = new Deliveries_Dispatches();
                                        deliveryDispatch.Created_At = dateOfEdit;
                                        deliveryDispatch.Dispatch_Id = (int)editDispatch.Id;
                                        deliveryDispatch.Delivery_Id = delivery.Id;
                                        _context.Deliveries_Dispatches.Add(deliveryDispatch);
                                        _context.SaveChanges();
                                    }
                                }

                            }

                            
                        }
                        _context.SaveChanges();

                        //Deleting orderPosition
                        List<int> listOfIdsToDelete = DispatchManager.GetIdstoRemove(editDispatch.DispatchPositions, dispatchPositionsFromDB);
                        foreach (var id in listOfIdsToDelete)
                        {
                            var dispatchPositionToDelete = _context.Dispatches_Positions.FirstOrDefault(d => d.Id == id && d.Deleted_At == null);
                            dispatchPositionToDelete.Deleted_At = dateOfEdit;
                            Dispatches_Positions dispatchPositionForParameter = _context.Dispatches_Positions.FirstOrDefault(d => d.Id == id && d.Deleted_At == null);
                            Orders_Positions orderPosition = _context.Orders_Positions.FirstOrDefault(o => o.Id == dispatchPositionForParameter.Order_Position_Id && o.Deleted_At == null);
                            Delivery delivery = _context.Deliveries.FirstOrDefault(d => d.Order_Id == orderPosition.Order_id && d.Deleted_At == null);
                            delivery.If_Delivery_Dispatch_Balanced = false;
                            _context.SaveChanges();


                            //sprawdzam czy jakiś orderPOsition z delivery dla tego dispatchPositions należy do tego dispatcha
                            bool isDeliveryDispatch = false;
                            List<int> orderPositionsForDeliveryIds = _context.Orders_Positions.Where(o => o.Order_id == delivery.Order_Id && o.Deleted_At == null).Select(o=>o.Id).ToList();
                            List<int> listOfdispatchPositionsIds = _context.Dispatches_Positions.Where(d => d.Dispatch_Id == editDispatch.Id && d.Deleted_At == null).Select(d=>d.Id).ToList();
                            foreach (var item in orderPositionsForDeliveryIds)
                            {
                                if (listOfdispatchPositionsIds.Contains(item))
                                {
                                    isDeliveryDispatch = true;
                                }
                            }

                            if (!isDeliveryDispatch)
                            {
                                Deliveries_Dispatches deliveryDispatch = _context.Deliveries_Dispatches.FirstOrDefault(d => d.Delivery_Id == delivery.Id && d.Dispatch_Id == editDispatch.Id && d.Deleted_At == null);
                                deliveryDispatch.Deleted_At = dateOfEdit;
                                _context.SaveChanges();
                            }
                        }

                        _context.SaveChanges();
                        dispatchToEdit.Number_Of_Positions = _context.Dispatches_Positions.Where(d => d.Dispatch_Id == editDispatch.Id && d.Deleted_At == null).Count();
                        _context.SaveChanges();

                    }
                    else
                    {
                        result.Status = false;
                        result.Message = "Dispatch not found";
                    }
                    result.Status = true;
                    result.Message = "Order has been edited";
                }
                catch (Exception ex)
                {
                    result.Status = false;
                    result.Message = ex.ToString();
                }
                return result;

            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
            }
        }

        [HttpGet]
        [Route("RemoveDispatch")]
        public RequestResult RemoveDispatch(int dispatchId)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin }))
            {
                RequestResult result = new RequestResult();
                try
                {
                    DateTime dateOfRemove = DateTime.Now;
                    Dispatch dispatchToRemove = _context.Dispatches.FirstOrDefault(o => o.Id == dispatchId && o.Deleted_At == null);
                    if (dispatchToRemove != null)
                    {
                        List<Dispatches_Positions> litOfDispatchesPositionsToRemove = _context.Dispatches_Positions.Where(o => o.Dispatch_Id == dispatchId && o.Deleted_At == null).ToList();
                        foreach (var item in litOfDispatchesPositionsToRemove)
                        {
                            item.Deleted_At = dateOfRemove;
                        }
                        dispatchToRemove.Deleted_At = dateOfRemove;

                       List<Deliveries_Dispatches> listOfDeliveryDispatches = _context.Deliveries_Dispatches.Where(d => d.Dispatch_Id == dispatchId && d.Deleted_At == null).ToList();
                        foreach (var item in listOfDeliveryDispatches)
                        {
                            Delivery deliveryToEdit = _context.Deliveries.FirstOrDefault(d => d.Id == item.Delivery_Id && d.Deleted_At == null);
                            deliveryToEdit.If_Delivery_Dispatch_Balanced = false;
                            deliveryToEdit.Edited_At = dateOfRemove;
                            item.Deleted_At = dateOfRemove;
                        }
                        
                        _context.SaveChanges();
                        result.Status = true;
                        result.Message = "Dispatch and his dispatch positions has been removed";
                        return result;
                    }
                    else
                    {
                        result.Status = false;
                        result.Message = "Dispatch not found";
                        return result;
                    }

                }
                catch (Exception ex)
                {
                    result.Status = false;
                    result.Message = ex.ToString();
                }
                return result;

            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
            }
        }

        [HttpPost]
        [Route("CreateDispatch")]
        public RequestResult CreateDispatch([FromBody]CreateDispatch newDispatch, bool isCMR)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin }))
            {
                RequestResult result = new RequestResult();
                try
                {
                    if (_context.Orders.OrderByDescending(o => o.Created_At).FirstOrDefault() == null || _context.Orders.OrderByDescending(o => o.Created_At).FirstOrDefault().Created_At.Value.Month != DateTime.Now.Month)
                    {
                        var counter = _context.Counters.FirstOrDefault(c => c.Name == "DispatchCounter");
                        counter.Count = 1;
                        _context.SaveChanges();
                    }
                    DateTime dateOfCreate = DateTime.Now;
                    Dispatch dispatchToAdd = new Dispatch();
                    CMR_Dispatches cmrDispatch = new CMR_Dispatches();
                    dispatchToAdd.Carrier_Address = newDispatch.Carrier.Carrier_Address;
                    dispatchToAdd.Carrier_Email = newDispatch.Carrier.Carrier_Email;
                    dispatchToAdd.Carrier_Name = newDispatch.Carrier.Carrier_Name;
                    dispatchToAdd.Carrier_VAT_Id = newDispatch.Carrier.Carrier_VAT_Id;
                    dispatchToAdd.Carrier_PhoneNumber = newDispatch.Carrier.Carrier_PhoneNumber;
                    dispatchToAdd.Car_Id = newDispatch.Car_Id;
                    dispatchToAdd.Creation_Date = dateOfCreate;
                    dispatchToAdd.Dispatch_Number = _context.Counters.FirstOrDefault(c => c.Name == "DispatchCounter").Count.ToString() + "/" + ((DateTime)dispatchToAdd.Creation_Date).Month.ToString() + "/" + ((DateTime)dispatchToAdd.Creation_Date).Year.ToString();
                    dispatchToAdd.Receiver_Address = newDispatch.Receiver.Receiver_Address;
                    dispatchToAdd.Receiver_Email = newDispatch.Receiver.Receiver_Email;
                    dispatchToAdd.Receiver_Name = newDispatch.Receiver.Receiver_Name;
                    dispatchToAdd.Receiver_VAT_Id = newDispatch.Receiver.Receiver_VAT_Id;
                    dispatchToAdd.Receiver_PhoneNumber = newDispatch.Receiver.Receiver_PhoneNumber;
                    dispatchToAdd.Created_At = dateOfCreate;
                    if (isCMR)
                    {
                        cmrDispatch.Created_At = dateOfCreate;
                        cmrDispatch.Destination = newDispatch.CMRDispatch.Destination;
                        cmrDispatch.Dispatch_Id = 0;
                        cmrDispatch.Sender_Address = newDispatch.CMRDispatch.Sender_Address;
                        cmrDispatch.Sender_Email = newDispatch.CMRDispatch.Sender_Email;
                        cmrDispatch.Sender_Name = newDispatch.CMRDispatch.Sender_Name;
                        cmrDispatch.Sender_VAT_Id = newDispatch.CMRDispatch.Sender_VAT_Id;
                        cmrDispatch.Dispatch_Id = dispatchToAdd.Id;
                        cmrDispatch.Commodity_Type = newDispatch.CMRDispatch.Commodity_Type;
                        _context.CMR_Dispatches.Add(cmrDispatch);
                        _context.SaveChanges();
                        dispatchToAdd.CMR_Id = cmrDispatch.Id == null ? null : cmrDispatch.Id.ToString();
                        _context.SaveChanges();
                    }
                    else
                    {
                        dispatchToAdd.CMR_Id = null;
                    }

                    dispatchToAdd.Number_Of_Positions = newDispatch.DispatchPositions.Count;
                    dispatchToAdd.If_PDF_And_Sent = false;
                    dispatchToAdd.If_CMR_And_Sent = false;
                    dispatchToAdd.If_CMR = isCMR;
                    dispatchToAdd.Duty_Doc_Id = newDispatch.Duty_Doc_Id;
                    dispatchToAdd.Creator_Id = UserHelper.GetCurrentUserId();
                    _context.Dispatches.Add(dispatchToAdd);
                    _context.SaveChanges();
                    foreach (var item in newDispatch.DispatchPositions)
                    {
                        Orders_Positions orderPosition = _context.Orders_Positions.FirstOrDefault(o => o.Id == item.Id && o.Deleted_At == null);
                        Dispatches_Positions dispatchPostion = new Dispatches_Positions();
                        dispatchPostion.Amount = item.Amount;
                        dispatchPostion.Created_At = dateOfCreate;
                        dispatchPostion.Dispatch_Id = dispatchToAdd.Id;
                        dispatchPostion.Order_Position_Id = item.Id;
                        dispatchPostion.Weight_Gross = item.Amount * orderPosition.Unit_Weight;
                        _context.Dispatches_Positions.Add(dispatchPostion);
                        _context.SaveChanges();


                        Delivery delivery = _context.Deliveries.FirstOrDefault(d => d.Order_Id == orderPosition.Order_id && d.Deleted_At == null);
                        if (_context.Deliveries_Dispatches.FirstOrDefault(d => d.Delivery_Id == delivery.Id && d.Dispatch_Id == dispatchToAdd.Id && d.Deleted_At == null) == null)
                        {
                            Deliveries_Dispatches deliveryDispatch = new Deliveries_Dispatches();
                            deliveryDispatch.Created_At = dateOfCreate;
                            deliveryDispatch.Dispatch_Id = (int)dispatchPostion.Dispatch_Id;
                            deliveryDispatch.Delivery_Id = delivery.Id;
                            _context.Deliveries_Dispatches.Add(deliveryDispatch);
                            _context.SaveChanges();
                        }

                        List<Orders_Positions> listOfOrdersPositionsForOrderPosition = _context.Orders_Positions.Where(o => o.Order_id == orderPosition.Order_id && o.Deleted_At==null).ToList();
                        bool isBalanced = true;
                        foreach (var orderPositionForOrder in listOfOrdersPositionsForOrderPosition)
                        {
                            List<Dispatches_Positions> dispatchPositions = _context.Dispatches_Positions.Where(d => d.Order_Position_Id == orderPositionForOrder.Id && d.Deleted_At==null).ToList();
                            if (orderPositionForOrder.Amount_Received != dispatchPositions.Sum(d => d.Amount))
                            {
                                isBalanced = false;
                            }
                        }
                        if (isBalanced)
                        {
                            delivery.If_Delivery_Dispatch_Balanced = true;
                        }
                    }
                    var orderCounter = _context.Counters.FirstOrDefault(c => c.Name == "DispatchCounter");
                    orderCounter.Count++;
                    _context.SaveChanges();
                    result.Status = true;
                    result.Message = "Order has been edited";

                }
                catch (Exception ex)
                {
                    result.Status = false;
                    result.Message = ex.ToString();
                }
                return result;

            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
            }
        }

        [HttpGet]
        [Route("GetDispatchPDF")]
        public byte[] GetDispatchPDF(int dispatchId, bool ifSendEmail = false)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin, (int)UserType.Client }))
            {
                try
                {
                    DispatchDetailsPDF dispatchInfoToPDF = DispatchManager.GetDispatchDetails(dispatchId);
                    Dispatch dispatch = _context.Dispatches.FirstOrDefault(d => d.Id == dispatchId && d.Deleted_At == null);
                    User userCreator = _context.Users.FirstOrDefault(u => u.Id == dispatch.Creator_Id && u.Deleted_at == null);
                    string creatorName = "";
                    if (userCreator != null)
                    {
                        creatorName = userCreator.Login;//zmienic na imie i nazwisko
                    }

                    byte[] result = _pdfManager.GenerateDispatchPDF(dispatchInfoToPDF, creatorName);
                    if (ifSendEmail)
                    {
                        _pdfManager.SendEmail("Dispatch_" + dispatch.Dispatch_Number, result);
                    }
                    return result;

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }

            }

            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
            }
        }

        [HttpGet]
        [Route("GetCMRPDF")]
        public byte[] GetCMRPDF(int dispatchId, bool ifSendEmail = false)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin, (int)UserType.Client }))
            {
                try
                {
                    Dispatch dispatchToPDF = _context.Dispatches.FirstOrDefault(d => d.Id == dispatchId && d.Deleted_At == null);
                    if (dispatchToPDF != null)
                    {
                        List<Dispatches_Positions> dispatchPositionsFromDB = _context.Dispatches_Positions.Where(d=>d.Dispatch_Id == dispatchId && d.Deleted_At == null).ToList();
                        int CMRId = Convert.ToInt32(dispatchToPDF.CMR_Id);
                        CMR_Dispatches CMRDispatchToPDF = _context.CMR_Dispatches.FirstOrDefault(c => c.Id == CMRId);
                        List<DispatchPositionsDispatchInfo> listOfDispatchPositionsToPDF = new List<DispatchPositionsDispatchInfo>();
                        foreach (var item in dispatchPositionsFromDB)
                        {
                            Orders_Positions orderPosition = _context.Orders_Positions.FirstOrDefault(o => o.Id == item.Order_Position_Id && o.Deleted_At == null);
                            if (orderPosition != null)
                            {
                                DispatchPositionsDispatchInfo dispatchPosition = new DispatchPositionsDispatchInfo();
                                dispatchPosition.Amount = item.Amount;
                                dispatchPosition.Weight_Gross = item.Weight_Gross;
                                dispatchPosition.Name = orderPosition.Name;
                                listOfDispatchPositionsToPDF.Add(dispatchPosition);

                            }
                            else
                            {
                                throw new Exception("Not found orderPosition for dispatchPosition");
                            }


                        }

                        byte[] result = _pdfManager.GenerateCMR(dispatchToPDF, listOfDispatchPositionsToPDF, CMRDispatchToPDF);
                        if (ifSendEmail)
                        {
                            _pdfManager.SendEmail("CMR"+dispatchToPDF.Dispatch_Number, result);
                        }
                        return result;
                    }
                    else
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Dispatch not found"));
                    }


                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }

            }

            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
            }
        }
    }
}
