using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class OrderPositionsDispatchInfoPDF
    {
        public int Id { get; set; }
        public string ATB { get; set; }
        public string Name { get; set; }
        public int? Amount_Received { get; set; }
        public decimal? Weight_Gross_Received { get; set; }
        public int? Amount_Before_Dispatch { get; set; }
        public decimal? Weight_Before_Dispatch { get; set; }
        public int? Amount_Dispatch{ get; set; }
        public decimal? Weight_Dispatch { get; set; }
    }
}