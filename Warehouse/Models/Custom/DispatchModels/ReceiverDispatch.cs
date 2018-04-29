using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class ReceiverDispatch
    {
        public string Receiver_Name { get; set; }
        public string Receiver_Address { get; set; }
        public string Receiver_VAT_Id { get; set; }
        public string Receiver_Email { get; set; }
    }
}