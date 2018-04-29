using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class OrderPositionsDeliveryInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
        public decimal Weight_Gross { get; set; }
        public int? Amount_Received { get; set; }
        public decimal? Weight_Gross_Received { get; set; }
    }
}