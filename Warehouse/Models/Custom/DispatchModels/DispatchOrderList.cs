using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class DispatchOrderList
    {
        public int OrderId {get;set;}
        public int DeliveryId { get; set; }
        public string ATB { get; set; }
        public List<DispatchPositionsDispatchInfo> ListOfDispatchPositions { get; set; }
    }
}