using System.Web.Mvc;

namespace Example.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(new
            {
                FavoriteColor = new
                {
                    Red = DB.Red,
                    Green = DB.Green,
                    Blue = DB.Blue
                }
            });
        }

        [HttpPost]
        public void Save(byte red, byte green, byte blue)
        {
            DB.Red = red;
            DB.Green = green;
            DB.Blue = blue;
        }
    }

    static class DB
    {
        public static byte Red, Green, Blue;
    }
}
