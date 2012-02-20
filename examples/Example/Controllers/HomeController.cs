﻿using System.Web.Mvc;

namespace Example.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(new 
            { 
                colorsUrl = Url.RouteUrl("Colors") 
            });
        }
    }
}

