using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class DispatchList
    {
        public int Id { get; set; }
        public string Creation_Date { get; set; }
        public string Car_Id { get; set; }
        public string Carrier_Name { get; set; }
        public string Receiver_Name { get; set; }
        public string Dispatch_Number { get; set; }
        public bool IsCMR { get; set; }

    }
}