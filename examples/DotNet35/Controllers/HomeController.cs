using System.Web.Mvc;

namespace DotNet35.Controllers
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