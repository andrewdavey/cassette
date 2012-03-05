using System.Web;
using System.Web.Routing;
using Cassette.Views;

namespace Cassette.Web.Jasmine
{
    class PageHandler : IHttpHandler
    {
        readonly HttpResponseBase response;
        readonly RouteData routeData;

        public PageHandler(RequestContext context)
        {
            response = context.HttpContext.Response;
            routeData = context.RouteData;
        }

        public void ProcessRequest(HttpContext _)
        {
            ProcessRequest();
        }

        void ProcessRequest()
        {
            ReferencePageBundles();

            var html = GetPageHtml();
            response.Write(html);
        }

        void ReferencePageBundles()
        {
            Bundles.Reference("~/cassette.web.jasmine");
            var spec = routeData.GetRequiredString("specbundle");
            Bundles.Reference(spec);
        }

        string GetPageHtml()
        {
            var html = Properties.Resources.runner;
            html = InsertStylesheetsIntoHtml(html);
            html = InsertScriptsIntoHtml(html);
            return html;
        }

        string InsertStylesheetsIntoHtml(string html)
        {
            var styles = Bundles.RenderStylesheets().ToHtmlString();
            return html.Replace("$styles$", styles);
        }

        string InsertScriptsIntoHtml(string html)
        {
            var scripts = Bundles.RenderScripts().ToHtmlString();
            return html.Replace("$scripts$", scripts);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}