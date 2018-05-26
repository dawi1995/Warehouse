using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class CreateOrder
    {
        public string Container_Id { get; set; }
        public string ATB { get; set; }
        public string Pickup_PIN { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string VAT_Id { get; set; }
        public string Email { get; set; }
        public string Terminal { get; set; }
        public DateTime ETA { get; set; }
        public List<CreateOrdersPositions> OrderPositions { get; set; }
    }
}