using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Example
{
    public static class Helpers
    {
        public static IHtmlString AssignPageViewData(this HtmlHelper html, object obj)
        {
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(obj);

            return new HtmlString(
                "<script type=\"text/javascript\">" +
                "window.pageViewData = " + json +
                "</script>"
            );
        }
    }
}