using System.Web.Mvc;

namespace Website.Controllers
{
    public class LicensingController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Buy()
        {
            return View();
        }

        public ActionResult EpicDiscount()
        {
            return View("ExpiredDiscount");
        }

        public ActionResult Purchased()
        {
            return View();
        }
    }
}