using System.Web.Mvc;

namespace Precompiled.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}