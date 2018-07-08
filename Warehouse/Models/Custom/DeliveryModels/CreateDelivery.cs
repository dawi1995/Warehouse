using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class CreateDelivery
    {
        public int Order_Id { get; set; }

        public string ATB { get; set; }
        public string Car_Id { get; set; }

        //public System.DateTime Date_Of_Delivery { get; set; }
        //public string Delivery_Number { get; set; }
        //public Nullable<bool> If_PDF_And_Sent { get; set; }
        //public Nullable<bool> If_PDF_Dispatch { get; set; }
        //public Nullable<bool> If_Differntial_Delivery_Dispatch { get; set; }
        //public Nullable<bool> If_PDF_Differential { get; set; }
        //public Nullable<bool> If_Delivery_Dispatch_Balanced { get; set; }
        //public Nullable<System.DateTime> Created_At { get; set; }
        //public Nullable<System.DateTime> Edited_At { get; set; }

        //public Nullable<System.DateTime> Deleted_At { get; set; }
        public List<CreateDeliveryPositions> DeliveryPositions { get; set; }
    }
}