using System.Web.Mvc;

namespace Website.Controllers
{
    public class DocumentationController : Controller
    {
        public ActionResult Index(string path)
        {
            string viewName;
            if (string.IsNullOrEmpty(path))
            {
                viewName = "Index";
            }
            else
            {
                viewName = path.Replace('/', '_').Replace("-", "");
            }

            var result = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
            if (result.View == null)
            {
                return HttpNotFound();
            }

            return View(viewName);
        }
    }
}
