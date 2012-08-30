using System.Text.RegularExpressions;
using System.Web;
using Cassette.Views;

namespace Cassette.Aspnet.Jasmine
{
    public class PageHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

        public void ProcessRequest(HttpContextBase context)
        {
            var match = Regex.Match(context.Request.PathInfo, "/run/(.*)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                SendRunnerPage(context, match);
            }
            else
            {
                SendHelpPage(context);
            }
        }

        void SendHelpPage(HttpContextBase context)
        {
            // baseUrl is probably jasmine.axd, but could have been changed in web.config handler definition.
            // So build it from the request data.
            var baseUrl = context.Request.ApplicationPath.TrimEnd('/') +
                          context.Request.AppRelativeCurrentExecutionFilePath.Substring(1);

            var html = Properties.Resources.help.Replace("$baseUrl$", baseUrl);
            context.Response.Write(html);
        }

        void SendRunnerPage(HttpContextBase context, Match match)
        {
            var specBundlePath = match.Groups[1].Value;
            ReferencePageBundles(specBundlePath);

            var html = GetPageHtml();
            context.Response.Write(html);
        }

        void ReferencePageBundles(string specBundlePath)
        {
            Bundles.Reference("~/cassette.aspnet.jasmine");
            Bundles.Reference(specBundlePath);
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