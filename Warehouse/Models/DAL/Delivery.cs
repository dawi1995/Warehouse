//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Warehouse.Models.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class Delivery
    {
        public int Id { get; set; }
        public int Order_Id { get; set; }
        public int Transport_Type { get; set; }
        public System.DateTime Date_Of_Delivery { get; set; }
        public string Delivery_Number { get; set; }
        public Nullable<bool> If_PDF_And_Sent { get; set; }
        public Nullable<bool> If_PDF_Dispatch { get; set; }
        public Nullable<bool> If_Differntial_Delivery_Dispatch { get; set; }
        public Nullable<bool> If_PDF_Differential { get; set; }
        public Nullable<bool> If_Delivery_Dispatch_Balanced { get; set; }
        public Nullable<System.DateTime> Created_At { get; set; }
        public Nullable<System.DateTime> Edited_At { get; set; }
        public Nullable<System.DateTime> Deleted_At { get; set; }
    }
}
