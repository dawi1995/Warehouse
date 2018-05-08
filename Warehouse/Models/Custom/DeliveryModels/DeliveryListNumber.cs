using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class DeliveryListNumber
    {
        public int NumberOfDeliveries { get; set; }
        public List<DeliveryList> ListOfDeliveries { get; set; }
    }
}