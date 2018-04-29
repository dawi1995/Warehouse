using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Warehouse.Controllers
{
    public class DeliveryController : ApiController
    {
        // GET: Delivery
        public ActionResult Index()
        {
            return View();
        }
    }
}