using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class OrderResultsList
    {
        public int NumberOfOrders { get; set; }
        public List<OrderResult> ListOfOrders { get; set; }
    }
}