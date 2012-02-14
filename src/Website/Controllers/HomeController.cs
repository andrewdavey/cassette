using System.Web.Mvc;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Web.Caching;
using System;

namespace Website.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Benefits()
        {
            return View();
        }

        public ActionResult Download()
        {
            return View();
        }

        public ActionResult Licensing()
        {
            return View();
        }

        public ActionResult Support()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Donate()
        {
            var contributors = HttpContext.Cache.Get("github.contributors") as IEnumerable<Contributor>;

            if (contributors == null)
            {
                var url = "https://api.github.com/repos/andrewdavey/cassette/contributors";
                var client = new WebClient();
                var json = client.DownloadString(url);
                contributors = JsonConvert.DeserializeObject<IEnumerable<Contributor>>(json);

                HttpContext.Cache.Insert("github.contributors", contributors, null, Cache.NoAbsoluteExpiration, TimeSpan.FromDays(1));
            }

            ViewBag.Contributors = contributors;
            return View();
        }
        
        public ActionResult Resources()
        {
            return View();
        }
    }

    public class Contributor
    {
        public string avatar_url { get; set; }
        public string login { get; set; }
        public string url { get; set; }
    }
}
