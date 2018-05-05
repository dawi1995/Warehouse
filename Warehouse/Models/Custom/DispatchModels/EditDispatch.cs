using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class EditDispatch
    {
        public int Id { get; set; }
        public string Dispatch_Number { get; set; }
        public DateTime? Creation_Date { get; set; }
        public string Car_Id { get; set; }
        public CarrierDispatch Carrier { get; set; }
        public ReceiverDispatch Receiver { get; set; }
        public List<EditDispatchPositions> DispatchPositions { get; set; }

        //public string Duty_Doc_Id { get; set; }
        //public Nullable<int> Number_Of_Positions { get; set; }
        //public string CMR_Id { get; set; }
        //public Nullable<bool> If_PDF_And_Sent { get; set; }
        //public Nullable<bool> If_CMR_And_Sent { get; set; }
        //public Nullable<bool> If_CMR { get; set; }
        //public Nullable<System.DateTime> Created_At { get; set; }
        //public Nullable<System.DateTime> Edited_At { get; set; }
        //public Nullable<System.DateTime> Deleted_At { get; set; }
    }
}