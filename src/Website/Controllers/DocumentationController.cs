using System.Web.Mvc;

namespace Website.Controllers
{
    public class DocumentationController : Controller
    {
        public ActionResult OldIndex(string path)
        {
            return RedirectToRoutePermanent("Documentation", new
            {
                version = "v1",
                path
            });
        }

        public ActionResult Index(string version, string path)
        {
            string viewName;
            if (string.IsNullOrEmpty(path))
            {
                viewName = "Index";
            }
            else
            {
                viewName = path.Replace("-", "");
            }
            
            viewName = "~/Views/Documentation/" + version + "/" + viewName + ".cshtml";

            var result = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
            if (result.View == null)
            {
                return HttpNotFound();
            }

            return View(viewName);
        }
    }
}
