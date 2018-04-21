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
    [Authorize]
    [RoutePrefix("api/Order")]
    public class OrderController : ApiController
    {
        private readonly WarehouseEntities _context;
        public OrderController()
        {
            _context = new WarehouseEntities();
        }

        /// <summary>
        /// Metoda do pobierania wszystkich zgłoszeń.
        /// </summary>
        /// <param name="offset">liczba pominiętych rekordów</param>
        /// <param name="limit">liczba jednorazowo pobranych rekordów</param>
        [HttpGet]
        [Route("GetAllOrders")]
        public List<OrderResult> GetAllOrders(int offset, int limit)
        {
            if (UserHelper.IsAuthorize(new List<int> { (int)UserType.SuperAdmin }))
            {
                try
                {
                    List<OrderResult> result = new List<OrderResult>();
                    var allOrders = _context.Orders.OrderByDescending(o => o.Creation_Date).Skip(offset).Take(limit);
                    foreach (var order in allOrders)
                    {
                        List<Orders_Positions> listOfOrderPositionsForOrder = new List<Orders_Positions>();
                        listOfOrderPositionsForOrder = _context.Orders_Positions.Where(o => o.Order_id == order.Id).ToList();
                        OrderResult orderResult = new OrderResult();
                        orderResult.Id = order.Id;
                        orderResult.Container_Id = order.Container_Id;
                        orderResult.ATB = order.ATB;
                        orderResult.Pickup_PIN = order.Pickup_PIN;
                        orderResult.Date_Of_Arrival = order.Date_Of_Arrival;
                        orderResult.Creation_Date = order.Creation_Date;
                        orderResult.Creator_Id = order.Creator_Id;
                        orderResult.Order_Number = order.Order_Number;
                        orderResult.Name = order.Name;
                        orderResult.Address = order.Address;
                        orderResult.VAT_Id = order.VAT_Id;
                        orderResult.Email = order.Email;
                        orderResult.Num_of_Positions = order.Num_of_Positions;
                        orderResult.If_PDF_And_Sent = order.If_PDF_And_Sent;
                        orderResult.If_Delivery_Generated = order.If_Delivery_Generated;
                        orderResult.Status = order.Status;
                        orderResult.Created_At = order.Created_At;
                        orderResult.Edited_At = order.Edited_At;
                        orderResult.Deleted_At = order.Deleted_At;
                        orderResult.OrdersPositions = listOfOrderPositionsForOrder;
                        result.Add(orderResult);
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