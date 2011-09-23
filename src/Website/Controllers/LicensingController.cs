using System.Web.Mvc;

namespace Website.Controllers
{
    public class LicensingController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Discount = false;
            ViewBag.Prices = new[] { 199, 399, 599, 799, 999 };
            ViewBag.PayPalButtonIds = new[]
            {
                "MXUPE7ASET542",
                "7PQ796D57WE34",
                "EN2MXRGFDY648",
                "HL5EG6F35JHD2",
                "AJK6SP49ZZWW8"
            };
            return View();
        }

        public ActionResult EpicDiscount()
        {
            ViewBag.Discount = true;
            ViewBag.Prices = new[] { 199, 399, 599, 799, 999 };
            ViewBag.DiscountPrices = new[] { 99, 199, 299, 399, 499 };
            ViewBag.PayPalButtonIds = new[]
            {
                "9YG9Y8XJ7FYJY",
                "EDBJRHYLZSVUC",
                "FZFUAP8G6UKHG",
                "6YY9H8P7UHUCC",
                "69L8TGD2ELJRN"
            };
            return View("Index");
        }

        public ActionResult Purchased()
        {
            return View();
        }
    }
}