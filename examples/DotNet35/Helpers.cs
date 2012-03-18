using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace DotNet35
{
    public static class Helpers
    {
        public static MvcHtmlString AssignPageViewData(this HtmlHelper html, object obj)
        {
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(obj);

            return MvcHtmlString.Create(
                "<script type=\"text/javascript\">" +
                "window.pageViewData = " + json +
                "</script>"
            );
        }
    }
}