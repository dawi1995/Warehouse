using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warehouse.Models.Custom
{
    public class DispatchListNumber
    {
        public int NumberOfDispatches { get; set; }
        public List<DispatchList> ListOfDispatches { get; set; }
    }
}