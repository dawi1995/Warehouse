using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class DeliveryList
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Container_Id { get; set; }
        public string ATB { get; set; }
        public string Date_Of_Delivery { get; set; }
        public string Name { get; set; }
        public bool IsDispatched { get; set; }
        public bool? IsDifferentDeliveryOrder { get; set; }
        public bool? IsBalancedDeliveryDispatch { get; set; }
    }
}