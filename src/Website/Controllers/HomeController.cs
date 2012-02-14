using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Caching;
using System.Web.Mvc;
using Newtonsoft.Json;

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
            var contributors = GetContributors();

            ViewBag.Contributors = contributors;
            return View();
        }

        IEnumerable<Contributor> GetContributors()
        {
            const string cacheKey = "github.contributors";
            
            var contributors = HttpContext.Cache.Get(cacheKey) as IEnumerable<Contributor>;
            if (contributors != null) return contributors;

            var json = DownoadContributorsJson();
            contributors = JsonConvert.DeserializeObject<IEnumerable<Contributor>>(json);
            
            HttpContext.Cache.Insert(
                cacheKey,
                contributors,
                null,
                Cache.NoAbsoluteExpiration,
                TimeSpan.FromDays(1)
            );

            return contributors;
        }

        static string DownoadContributorsJson()
        {
            using (var client = new WebClient())
            {
                return client.DownloadString("https://api.github.com/repos/andrewdavey/cassette/contributors");
            }
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