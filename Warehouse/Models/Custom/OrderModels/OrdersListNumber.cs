using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class OrdersListNumber
    {
        public int NumberOfOrders { get; set; }
        public List<OrdersList> ListOfOrders { get; set; }
    }
}