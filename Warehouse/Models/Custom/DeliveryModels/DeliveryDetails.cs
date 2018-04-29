using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class DeliveryDetails
    {
        public int Id { get; set; }
        public int Transport_Type { get; set; }
        public string Date_Of_Delivery { get; set; }
        public string Delivery_Number { get; set; }
        public List<OrderPositionsDeliveryInfo> ListOfOrderPositions { get; set; }
    }
}