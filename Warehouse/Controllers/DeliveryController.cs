using System;
using System.Collections.Generic;
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
        public DeliveryListNumber GetDeliveries(int offset = 0, int limit = int.MaxValue, string needle = "", bool isCreatingDispatch = false, int dispatchId = 0)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin}))
            {
                dynamic allDeliveries;
                if (!isCreatingDispatch)
                {
                     allDeliveries = (from deliveries in _context.Deliveries
                                     join orders in _context.Orders on deliveries.Order_Id equals orders.Id into q
                                     from orders in q.DefaultIfEmpty()
                                     where (deliveries.Deleted_At == null
                                     && (orders.ATB.Contains(needle) || orders.Container_Id.Contains(needle) || orders.Name.Contains(needle)))
                                     select new { Delivery = deliveries, Order = orders }).OrderByDescending(d => d.Delivery.Date_Of_Delivery).Skip(offset).Take(limit);
                }
                else
                {
                    if (dispatchId == 0)
                    {
                        allDeliveries = (from deliveries in _context.Deliveries
                                         join orders in _context.Orders on deliveries.Order_Id equals orders.Id into q
                                         from orders in q.DefaultIfEmpty()
                                         where (deliveries.Deleted_At == null
                                         && (orders.ATB.Contains(needle) || orders.Container_Id.Contains(needle) || orders.Name.Contains(needle))
                                         && deliveries.If_Delivery_Dispatch_Balanced == false)
                                         select new { Delivery = deliveries, Order = orders }).OrderByDescending(d => d.Delivery.Date_Of_Delivery).Skip(offset).Take(limit);
                    }
                    else
                    {
                        List<int> deliveryDispatchIds = _context.Deliveries_Dispatches.Where(d => d.Dispatch_Id == dispatchId && d.Deleted_At == null).Select(d => d.Delivery_Id).ToList();
                        allDeliveries = (from deliveries in _context.Deliveries
                                         join orders in _context.Orders on deliveries.Order_Id equals orders.Id into q
                                         from orders in q.DefaultIfEmpty()
                                         where (deliveries.Deleted_At == null
                                         && (orders.ATB.Contains(needle) || orders.Container_Id.Contains(needle) || orders.Name.Contains(needle))
                                         && (deliveries.If_Delivery_Dispatch_Balanced == false || deliveryDispatchIds.Contains(deliveries.Id)))
                                         select new { Delivery = deliveries, Order = orders }).OrderByDescending(d => d.Delivery.Date_Of_Delivery).Skip(offset).Take(limit);
                    }

                }
                //List<Delivery> listOfDeliveries = new List<Delivery>();


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

                        int deliveryId = item.Delivery.Id;
                        // gdzie masz te przedmioty, któe znajdują się w magazynie? Wyszukaj je funkcją strzałkową. Tylko, te które mają ID 
                        Deliveries_Dispatches deliveryDispatch = _context.Deliveries_Dispatches.FirstOrDefault(d => d.Delivery_Id == deliveryId && d.Deleted_At == null);
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
                    result.NumberOfDeliveries = DeliveryManager.CountOfDeliveries(needle, isCreatingDispatch, dispatchId);
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
                    result.Car_Id = deliveryFromDB.Car_Id;
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
                Regex ATBregex = new Regex("ATB[0-9]{18}");
                if (!ATBregex.IsMatch(createDelivery.ATB))
                {
                    result.Status = false;
                    result.Message = "ATB is in wrong format";
                    return result;
                }
                DateTime dateOfCreate = DateTime.Now;
                try
                {
                    if (_context.Deliveries.OrderByDescending(d => d.Created_At).FirstOrDefault() == null || _context.Deliveries.OrderByDescending(d => d.Created_At).FirstOrDefault().Created_At.Value.Month != DateTime.Now.Month)
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
                    newDelivery.Car_Id = createDelivery.Car_Id;
                    newDelivery.Creator_Id = UserHelper.GetCurrentUserId();
                    _context.Deliveries.Add(newDelivery);
                    foreach (var item in createDelivery.DeliveryPositions)
                    {
                        Orders_Positions orderPositionToEdit = _context.Orders_Positions.FirstOrDefault(o => o.Id == item.Id && o.Deleted_At == null);
                        if (orderPositionToEdit != null)
                        {
                            orderPositionToEdit.Edited_At = dateOfCreate;
                            orderPositionToEdit.Amount_Received = item.Amount;
                            orderPositionToEdit.Weight_Gross_Received = item.Amount * orderPositionToEdit.Unit_Weight;
                            if (orderPositionToEdit.Amount != item.Amount)
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

                        if(createDelivery.ATB != null)
                            orderToEdit.ATB = createDelivery.ATB;

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
        public RequestResult RemoveDelivery(int orderId)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin }))
            {
                RequestResult result = new RequestResult();
                try
                {
                    DateTime dateOfRemove = DateTime.Now;
                    Delivery deliveryToRemove = _context.Deliveries.FirstOrDefault(d => d.Order_Id == orderId && d.Deleted_At == null);
                    deliveryToRemove.Deleted_At = dateOfRemove;
                    Order orderToEdit = _context.Orders.FirstOrDefault(o => o.Id == deliveryToRemove.Order_Id && o.Deleted_At == null);
                    orderToEdit.Date_Of_Arrival = null;
                    orderToEdit.Edited_At = dateOfRemove;
                    orderToEdit.Status = (int)OrderStatus.Reported;
                    List<Orders_Positions> litOfOrdersPositionsToEdit = _context.Orders_Positions.Where(o => o.Order_id == orderToEdit.Id && o.Deleted_At == null).ToList();
                    foreach (var item in litOfOrdersPositionsToEdit)
                    {
                        item.Edited_At = dateOfRemove;
                        item.Amount_Received = null;
                        item.Weight_Gross_Received = null;
                    }
                    _context.SaveChanges();
                    result.Status = true;
                    result.Message = "Delivery and his orders positions has been removed";
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
                        deliveryToEdit.Car_Id = editDelivery.Car_Id;
                        deliveryToEdit.If_Delivery_Dispatch_Balanced = false;
                        deliveryToEdit.If_PDF_And_Sent = false;
                        deliveryToEdit.If_PDF_Differential = false;
                        deliveryToEdit.If_PDF_Dispatch = false;
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
                            orderPositionToEdit.Weight_Gross_Received = item.Amount* orderPositionToEdit.Unit_Weight;
                            if (orderPositionToEdit.Amount != item.Amount)
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
        public List<DeliveryState> GetDeliveryState(int orderId, int dispatchId = 0)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin, (int)UserType.Client }))
            {
                try
                {
                    List<DeliveryState> result = new List<DeliveryState>();
                    Delivery deliveryFromDB = _context.Deliveries.FirstOrDefault(d => d.Order_Id == orderId && d.Deleted_At == null);
                    if (deliveryFromDB != null)
                    {
                        List<int?> listOfDispatchesPositionsOrderPositionsIds = new List<int?>();
                        List<Orders_Positions> orderPositionFromDB = _context.Orders_Positions.Where(o => o.Order_id == orderId && o.Deleted_At == null).ToList();
                        if (dispatchId != 0)
                        {

                            listOfDispatchesPositionsOrderPositionsIds = _context.Dispatches_Positions.Where(d => d.Dispatch_Id == dispatchId && d.Deleted_At==null).Select(d=>d.Order_Position_Id).ToList();
                        }
                        foreach (var orderPosition in orderPositionFromDB)
                        {
                            DeliveryState deliveryState = new DeliveryState();
                            List<Dispatches_Positions> dispatchPositionsfromDB = _context.Dispatches_Positions.Where(d => d.Order_Position_Id == orderPosition.Id && d.Deleted_At == null).ToList();
                            int dispatchedAmount = dispatchPositionsfromDB == null ? 0 : (dispatchPositionsfromDB.Sum(d => d.Amount) ?? 0);
                            decimal dispatchedWeight =dispatchPositionsfromDB == null ? 0 : (dispatchPositionsfromDB.Sum(d => d.Weight_Gross) ?? 0);
                            deliveryState.Id = orderPosition.Id;
                            deliveryState.Name = orderPosition.Name;
                            deliveryState.Amount = (int)orderPosition.Amount_Received - dispatchedAmount;
                            if (listOfDispatchesPositionsOrderPositionsIds != null && listOfDispatchesPositionsOrderPositionsIds.Contains(orderPosition.Id))
                            {
                                Dispatches_Positions dispatchPositionsToAdd = _context.Dispatches_Positions.FirstOrDefault(d => d.Order_Position_Id == orderPosition.Id && d.Deleted_At==null);
                                deliveryState.Amount += (int)dispatchPositionsToAdd.Amount;
                            }

                            if (deliveryState.Amount > 0)
                            {
                                result.Add(deliveryState);
                            }
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
        public byte[] GetDeliveryPDF(int orderId, bool ifSendEmail)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin}))
            {
                try
                {
                    Delivery deliveryToPdf = _context.Deliveries.FirstOrDefault(d => d.Order_Id == orderId);
                    Order orderToPdf = _context.Orders.FirstOrDefault(o => o.Id == deliveryToPdf.Order_Id && o.Deleted_At == null);
                    List<Orders_Positions> orderPositionsToPdf = _context.Orders_Positions.Where(o => o.Order_id == deliveryToPdf.Order_Id && o.Deleted_At == null).ToList();
                    User userCreator = _context.Users.FirstOrDefault(u => u.Id == deliveryToPdf.Creator_Id && u.Deleted_at == null);
                    string creatorName = "";
                    if (userCreator != null)
                    {
                        creatorName = userCreator.Name + " " + userCreator.Surname;//Do zmiany na imie i nazwisko
                    }

                    byte[] result = _pdfManager.GenerateDeliveryPDF(deliveryToPdf, orderToPdf, orderPositionsToPdf, creatorName);
                    if (ifSendEmail)
                    {
                        _pdfManager.SendEmail("Delivery_" + deliveryToPdf.Delivery_Number, result);
                    }
                    // Zmienić creatora na creatora delivery czyli przyjmujacego zamowienie - trzeb dodać w bazie
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

        [HttpPost]
        [Route("GetDifferenceDeliveryPDF")]
        public byte[] GetDifferenceDeliveryPDF([FromBody]Committee commitee, int orderId, bool ifSendEmail = false)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin, (int)UserType.Admin }))
            {
                try
                {
                    Delivery deliveryToPdf = _context.Deliveries.FirstOrDefault(d => d.Order_Id == orderId && d.Deleted_At == null);
                    if (deliveryToPdf != null && !(bool)deliveryToPdf.If_Differential_Delivery_Order)
                    {
                        throw new Exception("This delivery is the same like order");
                    }
                    else
                    {
                        Order orderToPdf = _context.Orders.FirstOrDefault(o => o.Id == deliveryToPdf.Order_Id && o.Deleted_At == null);
                        List<Orders_Positions> orderPositionsToPdf = _context.Orders_Positions.Where(o => o.Order_id == deliveryToPdf.Order_Id && o.Deleted_At == null).ToList();
                        User userCreator = _context.Users.FirstOrDefault(u => u.Id == deliveryToPdf.Creator_Id && u.Deleted_at == null);
                        string creatorName = "";
                        if (userCreator != null)
                        {
                            creatorName = userCreator.Login;//Do zmiany na imie i nazwisko
                        }
                        byte[] result = _pdfManager.GenerateDifferenceDeliveryPDF(deliveryToPdf, orderToPdf, orderPositionsToPdf, commitee);
                        if (ifSendEmail)
                        {
                            _pdfManager.SendEmail("Difference_" + deliveryToPdf.Delivery_Number, result);
                        }
                        // Zmienić creatora na creatora delivery czyli przyjmujacego zamowienie - trzeb dodać w bazie
                        return result;
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