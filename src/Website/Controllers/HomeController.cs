using System.Web.Mvc;

namespace Website.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Download()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
    }
}