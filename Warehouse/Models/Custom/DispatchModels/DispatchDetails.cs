using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class DispatchDetails
    {
        public int Id { get; set; }
        public string Dispatch_Number { get; set; }
        public string Creation_Date { get; set; }
        public string Car_Id { get; set; }
        public CarrierDispatch Carrier { get; set; }
        public ReceiverDispatch Receiver { get; set; }
        public List<OrderPositionsDispatchInfo> ListOfOrderPositions { get; set; }
    }
}