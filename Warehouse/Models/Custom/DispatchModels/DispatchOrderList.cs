using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class DispatchOrderList
    {
        public string Order_Number {get;set;}
        public string Delivery_Number { get; set; }
        public string ContainerId { get; set; }
        public List<DispatchPositionsDispatchInfo> ListOfDispatchPositions { get; set; }
    }
}