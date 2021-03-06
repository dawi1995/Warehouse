﻿using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using Warehouse.Helpers;
using Warehouse.Managers;
using Warehouse.Models.Custom;
using Warehouse.Models.DAL;
using static Warehouse.Enums;

namespace Warehouse.Controllers
{
    [Authorize]
    [RoutePrefix("api/Order")]
    public class OrderController : ApiController
    {
        private readonly WarehouseEntities _context;
        private readonly PDFManager _pdfManager;
        private readonly OrderManager _orderManager;
        public OrderController()
        {
            _context = new WarehouseEntities();
            _pdfManager = new PDFManager();
            _orderManager = new OrderManager(_context);
        }

        [HttpGet]
        [Route("GetOrders")]
        public OrdersListNumber GetOrders(int offset = 0, int limit = int.MaxValue, string needle = "")
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin, (int)UserType.Client }))
            {
                OrdersListNumber result = new OrdersListNumber();
                List<Order> allOrders = new List<Order>();
                int currentUserId = UserHelper.GetCurrentUserId();
                try
                {
                    List<OrdersList> listOfOrderResult = new List<OrdersList>();
                    if (UserHelper.GetCurrentUserRole() == (int)UserType.SuperAdmin || UserHelper.GetCurrentUserRole() == (int)UserType.Admin)
                    {
                        allOrders = _context.Orders.Where(o => o.Deleted_At == null && (o.ATB.Contains(needle) || o.Name.Contains(needle) || o.Container_Id.Contains(needle))).OrderByDescending(o => o.Creation_Date).Skip(offset).Take(limit).ToList();
                    }
                    else
                    {
                        allOrders = _context.Orders.Where(o => o.Deleted_At == null && o.Creator_Id == currentUserId && (o.ATB.Contains(needle) || o.Name.Contains(needle) || o.Container_Id.Contains(needle))).OrderByDescending(o => o.Creation_Date).Skip(offset).Take(limit).ToList();
                    }

                    foreach (var order in allOrders)
                    {
                        OrdersList orderResult = new OrdersList();
                        orderResult.Id = order.Id;
                        orderResult.Container_Id = order.Container_Id;
                        orderResult.ATB = order.ATB;                    
                        orderResult.Creation_Date = order.Creation_Date == null ? string.Empty : ((DateTime)order.Creation_Date).ToString("dd-MM-yyyy");                    
                        orderResult.Status = order.Status;
                        orderResult.Name = order.Name;
                        orderResult.Terminal = order.Terminal;
                        orderResult.ReturnTerminal = order.ReturnTerminal;
                        var delivery = _context.Deliveries.FirstOrDefault(d => d.Order_Id == order.Id && d.Deleted_At == null);
                        if (delivery != null)
                        {
                            var deliveryDispatch = _context.Deliveries_Dispatches.FirstOrDefault(d => d.Delivery_Id == delivery.Id && d.Deleted_At == null);
                            if (deliveryDispatch != null)
                            {
                                orderResult.IsDispatched = true;
                            }
                            else
                            {
                                orderResult.IsDispatched = false;
                            }
                        }
                        else
                        {
                            orderResult.IsDispatched = false;
                        }
                        listOfOrderResult.Add(orderResult);
                    }

                    result.ListOfOrders = listOfOrderResult;
                    result.NumberOfOrders = _orderManager.CountOfOrders(needle);
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
        [Route("GetOrderDetails")]
        public OrderDetails GetOrderDetails(int orderId)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin, (int)UserType.Client }))
            {
                OrderDetails result = new OrderDetails();
                List<OrderPositionsOrderInfo> listOfOrderPositions = new List<OrderPositionsOrderInfo>();
                Orderer orderer = new Orderer();
                

                try
                {
                    List<Orders_Positions> ordersPositionsFromDB = _context.Orders_Positions.Where(o => o.Deleted_At == null && o.Order_id == orderId).OrderBy(o => o.Name).ToList();
                    Order orderFromDB = _context.Orders.FirstOrDefault(o => o.Id == orderId);
                    orderer.Name = orderFromDB.Name;
                    orderer.PrefixVat_Id = orderFromDB.PrefixVat_Id;
                    orderer.VAT_Id = orderFromDB.VAT_Id;
                    orderer.Address = orderFromDB.Address;
                    orderer.Email = orderFromDB.Email;
                    foreach (var orderPosition in ordersPositionsFromDB)
                    {
                        OrderPositionsOrderInfo toAdd = new OrderPositionsOrderInfo();
                        toAdd.Id = orderPosition.Id;
                        toAdd.Name = orderPosition.Name;
                        toAdd.Amount = orderPosition.Amount;
                        toAdd.Weight_Gross = orderPosition.Weight_Gross;
                        listOfOrderPositions.Add(toAdd);
                    }
                    result.Id = orderFromDB.Id;
                    result.Num_of_Positions = orderFromDB.Num_of_Positions;
                    result.Order_Number = orderFromDB.Order_Number;
                    result.Pickup_PIN = orderFromDB.Pickup_PIN;
                    result.ETA = orderFromDB.ETA == null ? string.Empty : orderFromDB.ETA.Value.ToString("dd-MM-yyyy");
                    result.Orderer = orderer;
                    result.Terminal = orderFromDB.Terminal;
                    result.ReturnTerminal = orderFromDB.ReturnTerminal;
                    result.ListOfOrderPositions = listOfOrderPositions;
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
        [Route("CreateOrder")]
        public RequestResult CreateOrder([FromBody]CreateOrder createOrder)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin, (int)UserType.Client }))
            {
                RequestResult result = new RequestResult();
                if (createOrder.ATB != null)
                {
                    Regex ATBregex = new Regex("ATB[0-9]{18}");
                    if (!ATBregex.IsMatch(createOrder.ATB))
                    {
                        result.Status = false;
                        result.Message = "ATB is in wrong format";
                        return result;
                    }
                }

                try
                {
                    if (_context.Orders.OrderByDescending(o => o.Created_At).FirstOrDefault() == null || _context.Orders.OrderByDescending(o => o.Created_At).FirstOrDefault().Created_At.Value.Month != DateTime.Now.Month)
                    {
                        var counter = _context.Counters.FirstOrDefault(c => c.Name == "OrderCounter");
                        counter.Count = 1;
                        _context.SaveChanges();
                    }
                    Order newOrder = new Order();
                    newOrder.Container_Id = createOrder.Container_Id.RemoveWhiteSpaces();
                    newOrder.ATB = createOrder.ATB.RemoveWhiteSpaces();
                    newOrder.Pickup_PIN = createOrder.Pickup_PIN;
                    newOrder.Creation_Date = DateTime.Now;
                    newOrder.Creator_Id = UserHelper.GetCurrentUserId();
                    newOrder.Order_Number = _context.Counters.FirstOrDefault(c => c.Name == "OrderCounter").Count.ToString() +"/"+ newOrder.Creation_Date.Month.ToString() + "/" + newOrder.Creation_Date.Year.ToString();
                    newOrder.Name = createOrder.Name;
                    newOrder.Address = createOrder.Address;
                    newOrder.PrefixVat_Id = createOrder.PrefixVat_Id;
                    newOrder.VAT_Id = createOrder.VAT_Id.RemoveWhiteSpaces();
                    newOrder.Email = createOrder.Email;
                    newOrder.ETA = createOrder.ETA;
                    newOrder.Num_of_Positions = createOrder.OrderPositions.Count;
                    newOrder.Terminal = createOrder.Terminal;
                    newOrder.ReturnTerminal = createOrder.ReturnTerminal;
                    newOrder.If_PDF_And_Sent = false;
                    newOrder.If_Delivery_Generated = false;
                    newOrder.Status = (int)OrderStatus.Reported;
                    newOrder.Created_At = newOrder.Creation_Date;
                    _context.Orders.Add(newOrder);
                    _context.SaveChanges();
                    foreach (var orderPosition in createOrder.OrderPositions)
                    {
                        Orders_Positions newOrderPosition = new Orders_Positions();
                        newOrderPosition.Order_id = newOrder.Id;
                        newOrderPosition.Name = orderPosition.Name;
                        newOrderPosition.Amount = orderPosition.Amount;
                        newOrderPosition.Weight_Gross = orderPosition.Weight_Gross;
                        newOrderPosition.Created_At = newOrder.Created_At;
                        newOrderPosition.Unit_Weight = orderPosition.Weight_Gross / orderPosition.Amount;
                        _context.Orders_Positions.Add(newOrderPosition);
                    }
                    var orderCounter = _context.Counters.FirstOrDefault(c => c.Name == "OrderCounter");
                    orderCounter.Count++;
                    _context.SaveChanges();
                    result.Status = true;
                    result.Message = "Order has been added";
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
        [Route("EditOrder")]
        public RequestResult EditOrder([FromBody]EditOrder editOrder)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin}))
            {
                DateTime dateOfEdit = DateTime.Now;
                RequestResult result = new RequestResult();
                if (editOrder.ATB != null)
                {
                    Regex ATBregex = new Regex("ATB[0-9]{18}");
                    if (!ATBregex.IsMatch(editOrder.ATB))
                    {
                        result.Status = false;
                        result.Message = "ATB is in wrong format";
                        return result;
                    }
                }

                try
                {
                    Order orderToEdit = _context.Orders.FirstOrDefault(o => o.Id == editOrder.Id);
                    orderToEdit.Container_Id = editOrder.Container_Id.RemoveWhiteSpaces();
                    orderToEdit.ATB = editOrder.ATB.RemoveWhiteSpaces();
                    orderToEdit.Pickup_PIN = editOrder.Pickup_PIN;
                    orderToEdit.Creation_Date = editOrder.Creation_Date;
                    orderToEdit.Order_Number = editOrder.Order_Number;
                    orderToEdit.Name = editOrder.Name;
                    orderToEdit.Address = editOrder.Address;
                    orderToEdit.PrefixVat_Id = editOrder.PrefixVat_Id;
                    orderToEdit.VAT_Id = editOrder.VAT_Id.RemoveWhiteSpaces();
                    orderToEdit.Email = editOrder.Email;
                    orderToEdit.ETA = editOrder.ETA;
                    orderToEdit.Terminal = editOrder.Terminal;
                    orderToEdit.ReturnTerminal = editOrder.ReturnTerminal;
                    //orderToEdit.Num_of_Positions = editOrder.Num_of_Positions;
                    orderToEdit.Edited_At = dateOfEdit;
                    _context.SaveChanges();
                    var orderPositionsFromDB = _context.Orders_Positions.Where(o => o.Order_id == editOrder.Id && o.Deleted_At == null).ToList();

                    //Editing and adding orderPositions
                    foreach (var orderPosition in editOrder.OrderPositions)
                    {
                        foreach (var orderPositionFromDB in orderPositionsFromDB)
                        {
                            if (orderPosition.Id == orderPositionFromDB.Id)
                            {
                                orderPositionFromDB.Amount = orderPosition.Amount;
                                orderPositionFromDB.Edited_At = dateOfEdit;
                                orderPositionFromDB.Name = orderPosition.Name;
                                orderPositionFromDB.Weight_Gross = orderPosition.Weight_Gross;
                                orderPositionFromDB.Unit_Weight = orderPosition.Weight_Gross / orderPosition.Amount;
                            }
                            if (orderPosition.Id == null)
                            {
                                Orders_Positions orderPositionsToAdd = new Orders_Positions();
                                orderPositionsToAdd.Created_At = dateOfEdit;
                                orderPositionsToAdd.Name = orderPosition.Name;
                                orderPositionsToAdd.Weight_Gross = orderPosition.Weight_Gross;
                                orderPositionsToAdd.Amount = orderPosition.Amount;
                                orderPositionsToAdd.Unit_Weight = orderPosition.Weight_Gross / orderPosition.Amount;
                                _context.Orders_Positions.Add(orderPositionsToAdd);
                            }
                        }
                    }
                    _context.SaveChanges();

                    //Deleting orderPosition
                    List<int> listOfIdsToDelete = _orderManager.GetIdstoRemove(editOrder.OrderPositions, orderPositionsFromDB);
                    foreach (var id in listOfIdsToDelete)
                    {
                        var orderPositionToDelete = _context.Orders_Positions.FirstOrDefault(o => o.Id == id && o.Deleted_At == null);
                        orderPositionToDelete.Deleted_At = dateOfEdit;
                    }

                    _context.SaveChanges();
                    orderToEdit.Num_of_Positions = _context.Orders_Positions.Where(o => o.Order_id == editOrder.Id && o.Deleted_At == null).Count();
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
        [Route("RemoveOrder")]
        public RequestResult RemoveOrder(int orderId)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin }))
            {
                RequestResult result = new RequestResult();
                try
                {
                    DateTime dateOfRemove = DateTime.Now;
                    Order orderToRemove = _context.Orders.FirstOrDefault(o => o.Id == orderId && o.Deleted_At == null);
                    if (orderToRemove != null)
                    {
                        List<Orders_Positions> litOfOrdersPositionsToRemove = _context.Orders_Positions.Where(o => o.Order_id == orderId && o.Deleted_At == null).ToList();
                        foreach (var item in litOfOrdersPositionsToRemove)
                        {
                            item.Deleted_At = dateOfRemove;
                        }
                        orderToRemove.Deleted_At = dateOfRemove;
                        _context.SaveChanges();
                        result.Status = true;
                        result.Message = "Order and his orders positions has been removed";
                        return result;
                    }
                    else
                    {
                        result.Status = false;
                        result.Message = "Order not found";
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

        [HttpGet]
        [Route("RemoveOrdersPosition")]
        public RequestResult RemoveOrdersPosition(int orderPositionId)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin }))
            {
                RequestResult result = new RequestResult();
                try
                {
                    DateTime dateOfRemove = DateTime.Now;
                    Orders_Positions orderPositionsToRemove = _context.Orders_Positions.FirstOrDefault(o => o.Id == orderPositionId);
                    orderPositionsToRemove.Deleted_At = dateOfRemove;
                    Order orderToUpdateNumOfPositions = _context.Orders.FirstOrDefault(o => o.Id == orderPositionsToRemove.Order_id);
                    orderToUpdateNumOfPositions.Num_of_Positions--;
                    _context.SaveChanges();
                    result.Status = true;
                    result.Message = "Orders positions has been removed and has been updated num of positions order";
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
        [Route("GetOrderPDF")]
        public byte[] GetOrderPDF(int orderId, bool ifSendEmail = false)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin, (int)UserType.Client }))
            {
                try
                {
                    Order orderToPdf = _context.Orders.FirstOrDefault(o => o.Id == orderId && o.Deleted_At == null);
                    List<Orders_Positions> orderPositionsToPdf = _context.Orders_Positions.Where(o => o.Id == orderId && o.Deleted_At==null).ToList();
                    User userCreator = _context.Users.FirstOrDefault(u => u.Id == orderToPdf.Creator_Id && u.Deleted_at == null);
                    string creatorName = "";
                    if(userCreator != null)
                    {
                        creatorName = userCreator.Name + " " + userCreator.Surname;//zmienic na imie i nazwisko
                    }
                    byte[] result = _pdfManager.GenerateOrderPDF(orderToPdf, orderPositionsToPdf, creatorName);
                    if (ifSendEmail)
                    {
                        _pdfManager.SendEmail("Order_" + orderToPdf.Order_Number, result);
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

    }
}