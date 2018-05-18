using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Warehouse.Helpers;
using Warehouse.Managers;
using Warehouse.Models.Custom;
using Warehouse.Models.DAL;
using static Warehouse.Enums;

namespace Warehouse.Controllers
{
    [RoutePrefix("api/Delivery")]
    public class DeliveryController : ApiController
    {

        private readonly WarehouseEntities _context;
        private readonly PDFManager _pdfManager;
        public DeliveryController()
        {
            _context = new WarehouseEntities();
            _pdfManager = new PDFManager();
        }

        [HttpGet]
        [Route("GetDeliveries")]
        public DeliveryListNumber GetDeliveries(int offset = 0, int limit = int.MaxValue, string needle = "")
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin}))
            {
                //List<Delivery> listOfDeliveries = new List<Delivery>();
                var allDeliveries = (from deliveries in _context.Deliveries
                               join orders in _context.Orders on deliveries.Order_Id equals orders.Id into q
                               from orders in q.DefaultIfEmpty()
                               where (deliveries.Deleted_At == null && (orders.ATB.Contains(needle) || orders.Container_Id.Contains(needle) || orders.Name.Contains(needle)))
                               select new { Delivery = deliveries, Order = orders }).OrderByDescending(d => d.Delivery.Date_Of_Delivery).Skip(offset).Take(limit);

                DeliveryListNumber result = new DeliveryListNumber();             
                try
                {
                    List<DeliveryList> listOfDeliveryResult = new List<DeliveryList>();

                    foreach (var item in allDeliveries)
                    {
                        DeliveryList deliveryResult = new DeliveryList();
                        deliveryResult.Id = item.Delivery.Id;
                        deliveryResult.OrderId = item.Delivery.Order_Id;
                        deliveryResult.ATB = item.Order.ATB;
                        deliveryResult.Container_Id = item.Order.Container_Id;
                        deliveryResult.Date_Of_Delivery = item.Delivery.Date_Of_Delivery == null? string.Empty : item.Delivery.Date_Of_Delivery.ToString("dd-MM-yyyy");
                        deliveryResult.Name = item.Order.Name;
                        deliveryResult.IsBalancedDeliveryDispatch = item.Delivery.If_Delivery_Dispatch_Balanced;
                        deliveryResult.IsDifferentDeliveryOrder = item.Delivery.If_Differential_Delivery_Order;

                        var deliveryDispatch = _context.Deliveries_Dispatches.FirstOrDefault(d => d.Delivery_Id == item.Delivery.Id && d.Deleted_At == null);
                        if (deliveryDispatch != null)
                        {
                            deliveryResult.IsDispatched = true;
                        }
                        else
                        {
                            deliveryResult.IsDispatched = false;
                        }

                        listOfDeliveryResult.Add(deliveryResult);
                    }

                    result.ListOfDeliveries = listOfDeliveryResult;
                    result.NumberOfDeliveries = DeliveryManager.CountOfDeliveries(needle);
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
        [Route("GetOrderDelivery")]
        public DeliveryDetails GetOrderDelivery(int orderId)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin, (int)UserType.Client }))
            {
                try
                {
                    DeliveryDetails result = new DeliveryDetails();
                    List<OrderPositionsDeliveryInfo> listOfOrderPositions = new List<OrderPositionsDeliveryInfo>();
                    Delivery deliveryFromDB = _context.Deliveries.FirstOrDefault(d => d.Order_Id == orderId && d.Deleted_At == null);
                    List<Orders_Positions> orderPositionFromDB = _context.Orders_Positions.Where(o => o.Order_id == orderId && o.Deleted_At == null).ToList();
                    foreach (var orderPosition in orderPositionFromDB)
                    {
                        OrderPositionsDeliveryInfo orderPositionDeliveryInfo = new OrderPositionsDeliveryInfo();
                        orderPositionDeliveryInfo.Id = orderPosition.Id;
                        orderPositionDeliveryInfo.Amount = orderPosition.Amount;
                        orderPositionDeliveryInfo.Amount_Received = orderPosition.Amount_Received;
                        orderPositionDeliveryInfo.Name = orderPosition.Name;
                        orderPositionDeliveryInfo.Weight_Gross = orderPosition.Weight_Gross;
                        orderPositionDeliveryInfo.Weight_Gross_Received = orderPosition.Weight_Gross_Received;
                        listOfOrderPositions.Add(orderPositionDeliveryInfo);
                    }
                    result.Id = deliveryFromDB.Id;
                    result.Date_Of_Delivery = deliveryFromDB.Date_Of_Delivery == null ? string.Empty : ((DateTime)deliveryFromDB.Date_Of_Delivery).ToString("dd-MM-yyyy");
                    result.Delivery_Number = deliveryFromDB.Delivery_Number;
                    result.Transport_Type = deliveryFromDB.Transport_Type;
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
        [Route("CreateDelivery")]
        public RequestResult CreateDelivery([FromBody]CreateDelivery createDelivery)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin, (int)UserType.Client }))
            {
                bool isDifferent = false;
                RequestResult result = new RequestResult();
                DateTime dateOfCreate = DateTime.Now;
                try
                {
                    if (_context.Deliveries.OrderByDescending(d => d.Created_At).FirstOrDefault().Created_At.Value.Month != DateTime.Now.Month)
                    {
                        var counter = _context.Counters.FirstOrDefault(c => c.Name == "DeliveryCounter");
                        counter.Count = 1;
                        _context.SaveChanges();
                    }
                    Delivery newDelivery = new Delivery();
                    newDelivery.Created_At = dateOfCreate;
                    newDelivery.Date_Of_Delivery = dateOfCreate;
                    newDelivery.Delivery_Number = _context.Counters.FirstOrDefault(c => c.Name == "DeliveryCounter").Count.ToString() +"/" +newDelivery.Date_Of_Delivery.Month.ToString() + "/" + newDelivery.Date_Of_Delivery.Year.ToString();
                    newDelivery.If_Delivery_Dispatch_Balanced = false;
                    newDelivery.If_PDF_And_Sent = false;
                    newDelivery.If_PDF_Differential = false;
                    newDelivery.If_PDF_Dispatch = false;
                    newDelivery.Order_Id = createDelivery.Order_Id;
                    newDelivery.Transport_Type = createDelivery.Transport_Type;
                    _context.Deliveries.Add(newDelivery);
                    foreach (var item in createDelivery.DeliveryPositions)
                    {
                        Orders_Positions orderPositionToEdit = _context.Orders_Positions.FirstOrDefault(o => o.Id == item.Id && o.Deleted_At == null);
                        if (orderPositionToEdit != null)
                        {
                            orderPositionToEdit.Edited_At = dateOfCreate;
                            orderPositionToEdit.Amount_Received = item.Amount;
                            orderPositionToEdit.Weight_Gross_Received = item.Weight_Gross;
                            if (orderPositionToEdit.Amount != item.Amount || orderPositionToEdit.Weight_Gross != item.Weight_Gross)
                            {
                                isDifferent = true;
                            }
                        }
                        else
                        {
                            result.Status = false;
                            result.Message = "Order Position not found";
                            return result;
                        }
                    }
                    Order orderToEdit = _context.Orders.FirstOrDefault(o => o.Id == createDelivery.Order_Id && o.Deleted_At == null);
                    if (orderToEdit != null)
                    {
                        orderToEdit.Date_Of_Arrival = dateOfCreate;
                        orderToEdit.Edited_At = dateOfCreate;
                        orderToEdit.If_Delivery_Generated = true;
                        if (isDifferent)
                        {
                            newDelivery.If_Differential_Delivery_Order = true;
                            orderToEdit.Status = (int)OrderStatus.Difference;
                        }
                        else
                        {
                            newDelivery.If_Differential_Delivery_Order = false;
                            orderToEdit.Status = (int)OrderStatus.Accepted;
                        }

                        var deliveryCounter = _context.Counters.FirstOrDefault(c => c.Name == "DeliveryCounter");
                        deliveryCounter.Count++;
                        _context.SaveChanges();
                        result.Status = true;
                        result.Message = "Delivery has been added";
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
                    return result;
                }
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
            }
        }

        [HttpGet]
        [Route("RemoveDelivery")]
        public RequestResult RemoveOrder(int orderId)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin }))
            {
                RequestResult result = new RequestResult();
                try
                {
                    DateTime dateOfRemove = DateTime.Now;
                    Order orderToRemove = _context.Orders.FirstOrDefault(o => o.Id == orderId);
                    List<Orders_Positions> litOfOrdersPositionsToRemove = _context.Orders_Positions.Where(o => o.Order_id == orderId && o.Deleted_At == null).ToList();
                    foreach (var item in litOfOrdersPositionsToRemove)
                    {
                        item.Deleted_At = dateOfRemove;
                    }
                    orderToRemove.Deleted_At = dateOfRemove;
                    _context.SaveChanges();
                    result.Status = true;
                    result.Message = "Order and his orders positions has been removed";
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
        [Route("EditDelivery")]
        public RequestResult EditDelivery([FromBody]EditDelivery editDelivery)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin}))
            {
                bool isDifferent = false;
                RequestResult result = new RequestResult();
                DateTime dateOfEdit = DateTime.Now;
                try
                {
                    if (_context.Deliveries.OrderByDescending(d => d.Created_At).FirstOrDefault().Created_At.Value.Month != DateTime.Now.Month)
                    {
                        var counter = _context.Counters.FirstOrDefault(c => c.Name == "DeliveryCounter");
                        counter.Count = 1;
                        _context.SaveChanges();
                    }
                    Delivery deliveryToEdit = _context.Deliveries.FirstOrDefault(d => d.Id == editDelivery.Id && d.Deleted_At == null);
                    if (deliveryToEdit != null)
                    {
                        deliveryToEdit.Edited_At = dateOfEdit;
                        deliveryToEdit.Date_Of_Delivery = editDelivery.Date_Of_Delivery;
                        deliveryToEdit.Delivery_Number = editDelivery.Delivery_Number;
                        deliveryToEdit.If_Delivery_Dispatch_Balanced = false;
                        deliveryToEdit.If_PDF_And_Sent = false;
                        deliveryToEdit.If_PDF_Differential = false;
                        deliveryToEdit.If_PDF_Dispatch = false;
                        deliveryToEdit.Transport_Type = editDelivery.Transport_Type;
                    }
                    else
                    {
                        result.Status = false;
                        result.Message = "Delivery not found";
                        return result;
                    }
                   
                    foreach (var item in editDelivery.DeliveryPositions)
                    {
                        Orders_Positions orderPositionToEdit = _context.Orders_Positions.FirstOrDefault(o => o.Id == item.Id && o.Deleted_At == null);
                        if (orderPositionToEdit != null)
                        {
                            orderPositionToEdit.Edited_At = dateOfEdit;
                            orderPositionToEdit.Amount_Received = item.Amount;
                            orderPositionToEdit.Weight_Gross_Received = item.Weight_Gross;
                            if (orderPositionToEdit.Amount != item.Amount || orderPositionToEdit.Weight_Gross != item.Weight_Gross)
                            {
                                isDifferent = true;
                            }
                        }
                        else
                        {
                            result.Status = false;
                            result.Message = "Order Position not found";
                            return result;
                        }
                    }
                    
                    Order orderToEdit = _context.Orders.FirstOrDefault(o => o.Id == deliveryToEdit.Order_Id && o.Deleted_At == null);
                    if (orderToEdit != null)
                    {
                        orderToEdit.Date_Of_Arrival = editDelivery.Date_Of_Delivery;
                        orderToEdit.Edited_At = dateOfEdit;
                        orderToEdit.If_Delivery_Generated = true;
                        if (isDifferent)
                        {
                            deliveryToEdit.If_Differential_Delivery_Order = true;
                            orderToEdit.Status = (int)OrderStatus.Difference;
                        }
                        else
                        {
                            deliveryToEdit.If_Differential_Delivery_Order = false;
                            orderToEdit.Status = (int)OrderStatus.Accepted;
                        }

                        var deliveryCounter = _context.Counters.FirstOrDefault(c => c.Name == "DeliveryCounter");
                        deliveryCounter.Count++;
                        _context.SaveChanges();
                        result.Status = true;
                        result.Message = "Delivery has been added";
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
                    return result;
                }
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User don't have acces to this method"));
            }
        }

        [HttpGet]
        [Route("GetDeliveryState")]
        public List<DeliveryState> GetDeliveryState(int orderId)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin, (int)UserType.Client }))
            {
                try
                {
                    List<DeliveryState> result = new List<DeliveryState>();
                    Delivery deliveryFromDB = _context.Deliveries.FirstOrDefault(d => d.Order_Id == orderId && d.Deleted_At == null);
                    if (deliveryFromDB != null)
                    {
                        List<Orders_Positions> orderPositionFromDB = _context.Orders_Positions.Where(o => o.Order_id == orderId && o.Deleted_At == null).ToList();

                        foreach (var orderPosition in orderPositionFromDB)
                        {
                            DeliveryState deliveryState = new DeliveryState();
                            List<Dispatches_Positions> dispatchPositionsfromDB = _context.Dispatches_Positions.Where(d => d.Order_Position_Id == orderPosition.Id && d.Deleted_At == null).ToList();
                            int dispatchedAmount = dispatchPositionsfromDB == null ? 0 : (dispatchPositionsfromDB.Sum(d => d.Amount) ?? 0);
                            decimal dispatchedWeight =dispatchPositionsfromDB == null ? 0 : (dispatchPositionsfromDB.Sum(d => d.Weight_Gross) ?? 0);
                            deliveryState.Id = orderPosition.Id;
                            deliveryState.Name = orderPosition.Name;
                            deliveryState.Amount = (int)orderPosition.Amount_Received - dispatchedAmount;
                            deliveryState.Weight_Gross = (decimal)orderPosition.Weight_Gross_Received - dispatchedWeight;
                            result.Add(deliveryState);
                        }
                        return result;
                    }
                    else
                    {
                        throw new Exception("Not found delivery for this order id.");
                       // throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Not found delivery for this order id."));
                    }
                   
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
        [Route("GetDeliveryPDF")]
        public byte[] GetDeliveryPDF(int deliveryId)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin}))
            {
                try
                {
                    Delivery deliveryToPdf = _context.Deliveries.FirstOrDefault(d => d.Id == deliveryId);
                    Order orderToPdf = _context.Orders.FirstOrDefault(o => o.Id == deliveryToPdf.Order_Id && o.Deleted_At == null);
                    List<Orders_Positions> orderPositionsToPdf = _context.Orders_Positions.Where(o => o.Order_id == deliveryToPdf.Order_Id && o.Deleted_At == null).ToList();
                    Client clientCreator = _context.Clients.FirstOrDefault(c => c.User_Id == orderToPdf.Creator_Id && c.Deleted_At == null);
                    string creatorName = "";
                    if (clientCreator != null)
                    {
                        creatorName = clientCreator.Name;
                    }
                    // Zmienić creatora na creatora delivery czyli przyjmujacego zamowienie - trzeb dodać w bazie
                    return _pdfManager.GenerateDeliveryPDF(deliveryToPdf, orderToPdf, orderPositionsToPdf, creatorName);

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