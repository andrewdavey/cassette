using System.Linq;
using System.Web.Mvc;
using Example.Models;

namespace Example.Controllers
{
    public class ColorController : Controller
    {
        // Demo code - please don't do this for real!
        static ColorRepository colors = new ColorRepository();

        public ActionResult List()
        {
            return Json(
                colors.Select(color => new 
                {
                    url = Url.RouteUrl("Color", new { id = color.Id }),
                    red = color.Red,
                    green = color.Green,
                    blue = color.Blue
                }), 
                JsonRequestBehavior.AllowGet
            );
        }

        [HttpPost, ActionName("list")]
        public ActionResult Add(byte red, byte green, byte blue)
        {
            var newColor = colors.Add(red, green, blue);

            var colorUrl = Url.RouteUrl("Color", new { id = newColor.Id });
            Response.StatusCode = 201; // Created
            Response.AddHeader("Location", colorUrl);
            return new EmptyResult();
        }

        [HttpDelete, ActionName("item")]
        public void Delete(int id)
        {
            colors.Delete(id);
        }
    }
}
