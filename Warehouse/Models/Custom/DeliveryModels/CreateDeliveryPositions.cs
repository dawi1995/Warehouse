using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class CreateDeliveryPositions
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public decimal Weight_Gross { get; set; }
    }
}