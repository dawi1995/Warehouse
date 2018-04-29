using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class DeliveryDetails
    {
        public int Id { get; set; }
        public string Transport_Type { get; set; }
        public DateTime Date_Of_Delivery { get; set; }
    }
}