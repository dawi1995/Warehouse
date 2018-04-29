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
                    Delivery deliveryFromDB = _context.Deliveries.FirstOrDefault(d => d.Order_Id == orderId);
                    List<Orders_Positions> orderPositionFromDB = _context.Orders_Positions.Where(o => o.Order_id == orderId).ToList();
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
    }
}