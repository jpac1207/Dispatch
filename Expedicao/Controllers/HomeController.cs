using Expedicao.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Expedicao.Controllers
{
    [AutenticationFilter]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Sistema De Expedições Eletroeletrônica.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Fale Conosco";

            return View();
        }
    }
}