using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Warehouse.Helpers;
using Warehouse.Models.Custom;
using Warehouse.Models.DAL;
using static Warehouse.Enums;

namespace Warehouse.Controllers
{
    [RoutePrefix("api/Delivery")]
    public class DeliveryController : ApiController
    {

        private readonly WarehouseEntities _context;
        public DeliveryController()
        {
            _context = new WarehouseEntities();
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
                    newDelivery.Delivery_Number = newDelivery.Date_Of_Delivery.Year.ToString() + "/" + newDelivery.Date_Of_Delivery.Month.ToString() + "/" + _context.Counters.FirstOrDefault(c => c.Name == "DeliveryCounter").Count.ToString();
                    newDelivery.If_Delivery_Dispatch_Balanced = false;
                    newDelivery.If_PDF_And_Sent = false;
                    newDelivery.If_PDF_Differential = false;
                    newDelivery.If_PDF_Dispatch = false;
                    newDelivery.Order_Id = createDelivery.Order_Id;
                    newDelivery.Transport_Type = createDelivery.Transport_Type;
                    foreach (var item in createDelivery.DeliveryPositions)
                    {
                        Orders_Positions orderPositionToEdit = _context.Orders_Positions.FirstOrDefault(o => o.Id == item.OrderPositionId && o.Deleted_At == null);
                        if (orderPositionToEdit != null)
                        {
                            orderPositionToEdit.Edited_At = dateOfCreate;
                            orderPositionToEdit.Amount_Received = item.Amount;
                            orderPositionToEdit.Weight_Gross_Received = item.Weight_Gross;
                            if (orderPositionToEdit.Amount_Received != item.Amount || orderPositionToEdit.Weight_Gross != item.Weight_Gross)
                            {
                                isDifferent = true;
                            }
                        }
                        else
                        {
                            result.Status = true;
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
                            newDelivery.If_Differntial_Delivery_Dispatch = true;
                            orderToEdit.Status = (int)OrderStatus.Difference;
                        }
                        else
                        {
                            newDelivery.If_Differntial_Delivery_Dispatch = false;
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
                        result.Status = true;
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
    }
}