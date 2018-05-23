using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class DispatchDetailsDispatchInfo
    {
        public int Id { get; set; }
        public string Dispatch_Number { get; set; }
        public string Duty_Doc_Id { get; set; }
        public string Car_Id { get; set; }
        public bool? isCMR { get; set; }
        public CMRDispatch CmrDispatch {get; set;}
        public CarrierDispatch Carrier { get; set; }
        public ReceiverDispatch Receiver { get; set; }
        public List<DispatchOrderList> ListOfDispatchOrders { get; set; }

    }
}