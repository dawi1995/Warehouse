using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Warehouse.Models.DAL;

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
        [Route("GetOrderDeliveries")]
        public OrderDetails GetOrderDeliveries(int orderId)
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
                    result.Creator_Id = orderFromDB.Creator_Id;
                    result.Num_of_Positions = orderFromDB.Num_of_Positions;
                    result.Order_Number = orderFromDB.Order_Number;
                    result.Pickup_PIN = orderFromDB.Pickup_PIN;
                    result.Orderer = orderer;
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