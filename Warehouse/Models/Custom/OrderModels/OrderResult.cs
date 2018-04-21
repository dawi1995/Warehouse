using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Warehouse.Models.DAL;

namespace Warehouse.Models.Custom
{
    public class OrderResult
    {
        public int Id { get; set; }
        public string Container_Id { get; set; }
        public string ATB { get; set; }
        public string Pickup_PIN { get; set; }
        public DateTime? Date_Of_Arrival { get; set; }
        public DateTime Creation_Date { get; set; }
        public int Creator_Id { get; set; }
        public string Order_Number { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string VAT_Id { get; set; }
        public string Email { get; set; }
        public int Num_of_Positions { get; set; }
        public bool? If_PDF_And_Sent { get; set; }
        public bool? If_Delivery_Generated { get; set; }
        public string Status { get; set; }
        public DateTime? Created_At { get; set; }
        public DateTime? Edited_At { get; set; }
        public DateTime? Deleted_At { get; set; }
        public List<Orders_Positions> OrdersPositions { get; set; }
    }
}