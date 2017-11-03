using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TravelPlanner.Controllers
{
    public class HomeController : Controller
    {
        // GET: TravelPlanner
        public ActionResult Index()
        {
            return View();
        }
    }
}