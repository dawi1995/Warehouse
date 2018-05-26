using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class CarrierDispatch
    {
        public string Carrier_Name { get; set; }
        public string Carrier_Address { get; set; }
        public string Carrier_VAT_Id { get; set; }
        public string Carrier_Email { get; set; }
        public string Carrier_PhoneNumber { get; set; }
    }
}