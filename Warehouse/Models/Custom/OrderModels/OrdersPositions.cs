using System;
using System.Collections.Generic;

namespace Warehouse.Models.Custom
{
    public class OrdersPositions
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order_id { get; set; }
        public int Amount { get; set; }
        public int Weight_Gross { get; set; }
        public int? Amount_Received { get; set; }
        public int? Weight_Gross_Received { get; set; }
        public string Created_At { get; set; }
        public string Edited_At { get; set; }
    }
}