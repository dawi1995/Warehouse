using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class CMRDispatch
    {
        public string Commodity_Type { get; set; }
        public string Destination { get; set; }
        public string Sender_Name { get; set; }
        public string Sender_Address { get; set; }
        public string Sender_PrefixVat_Id { get; set; }
        public string Sender_VAT_Id { get; set; }
        public string Sender_Email { get; set; }
    }
}