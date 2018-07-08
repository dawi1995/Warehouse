using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class EditDelivery
    {
        public int Id { get; set; }
        public DateTime Date_Of_Delivery { get; set; }
        public string Delivery_Number { get; set; }
        public string Car_Id { get; set; }

        public List<EditDeliveryPositions> DeliveryPositions { get; set; }
    }
}