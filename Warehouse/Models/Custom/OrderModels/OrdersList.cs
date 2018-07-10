using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class OrdersList
    {
        public int Id { get; set; }
        public string Container_Id { get; set; }
        public string ATB { get; set; }
        public string Creation_Date { get; set; }
        public string Name { get; set; }
        public int? Status { get; set; }
        public string ReturnTerminal { get; set; }
        public string Terminal { get; set; }
        public bool IsDispatched { get; set; }
    }
}