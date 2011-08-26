using System.Web.Mvc;

namespace Website.Controllers
{
    public class DocumentationController : Controller
    {
        public ActionResult Index(string path)
        {
            if (string.IsNullOrEmpty(path)) path = "GettingStarted";
            path = path.Replace('/', '_').Replace("-", "");

            var result = ViewEngines.Engines.FindPartialView(ControllerContext, path);
            if (result.View == null)
            {
                return HttpNotFound();
            }

            ViewBag.ChildView = path;
            return View();
        }
    }
}