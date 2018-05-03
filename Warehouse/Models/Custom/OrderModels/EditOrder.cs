using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class EditOrder
    {
        public int Id { get; set; }
        public string Container_Id { get; set; }
        public string ATB { get; set; }
        public string Pickup_PIN { get; set; }
        public DateTime Creation_Date { get; set; }
        public string Order_Number { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string VAT_Id { get; set; }
        public string Email { get; set; }
        public List<EditOrdersPositions> OrderPositions { get; set; }
    }
}