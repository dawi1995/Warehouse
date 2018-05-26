using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class OrderDetails
    {
        public int Id { get; set; }
        public string Pickup_PIN { get; set; }
        public string Order_Number { get; set; }
        public int Num_of_Positions { get; set; }
        public string ETA { get; set; }
        public string Terminal { get; set; }
        public Orderer Orderer { get; set; }
        public List<OrderPositionsOrderInfo> ListOfOrderPositions { get; set; }
    }
}