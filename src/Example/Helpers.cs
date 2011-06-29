using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Example
{
    public static class Helpers
    {
        public static IHtmlString ToJson(this HtmlHelper html, object obj)
        {
            var serializer = new JavaScriptSerializer();
            return new HtmlString(serializer.Serialize(obj));
        }
    }
}