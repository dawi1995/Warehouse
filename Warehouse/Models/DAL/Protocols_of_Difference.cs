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
    
    public partial class Protocols_of_Difference
    {
        public int Id { get; set; }
        public string Protocol_Of_Difference_Id { get; set; }
        public int Delivery_Id { get; set; }
        public Nullable<System.DateTime> Created_At { get; set; }
        public Nullable<System.DateTime> Edited_At { get; set; }
        public Nullable<System.DateTime> Deleted_At { get; set; }
    }
}
