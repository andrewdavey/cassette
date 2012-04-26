using System.Text.RegularExpressions;
using System.Web;
using TinyIoC;

namespace Cassette.Aspnet
{
    /// <summary>
    /// Handles requests for Cassette bundles, assets and diagnostic page.
    /// </summary>
    public class CassetteHttpHandler : IHttpHandler
    {
        readonly TinyIoCContainer requestContainer;
        readonly HttpRequestBase request;

        /// <summary>
        /// Default public constructor used by ASP.NET to create the handler.
        /// </summary>
        public CassetteHttpHandler()
            : this(
            CassetteHttpModule.Host.RequestContainer, 
            CassetteHttpModule.Host.RequestContainer.Resolve<HttpRequestBase>()
            )
        {
        }

        public CassetteHttpHandler(TinyIoCContainer requestContainer, HttpRequestBase request)
        {
            this.requestContainer = requestContainer;
            this.request = request;
        }

        public void ProcessRequest()
        {
            var pathInfo = request.PathInfo;
            if (string.IsNullOrEmpty(pathInfo))
            {
                CallDiagnosticHandler();
            }
            else
            {
                CallPathInfoHandler(pathInfo);
            }
        }

        void CallDiagnosticHandler()
        {
            var diagnosticHandler = requestContainer.Resolve<IDiagnosticRequestHandler>();
            diagnosticHandler.ProcessRequest();
        }

        void CallPathInfoHandler(string pathInfo)
        {
            var match = Regex.Match(pathInfo, "/(?<type>[^/]+)/(?<hash>[^/]+)/(?<path>.*)");
            var type = match.Groups["type"].Value;
            var handler = CreateRequestHandler(type);
            var path = "~/" + match.Groups["path"].Value;
            handler.ProcessRequest(path);
        }

        ICassetteRequestHandler CreateRequestHandler(string type)
        {
            switch (type.ToLowerInvariant())
            {
                // Note that "cassette.axd/file/{hash}/path" paths are also valid, but are rewritten to let IIS handle them.
                // So they never get processed by this handler.

                case "script":
                    return requestContainer.Resolve<ICassetteRequestHandler>("ScriptBundleRequestHandler");

                case "stylesheet":
                    return requestContainer.Resolve<ICassetteRequestHandler>("StylesheetBundleRequestHandler");

                case "htmltemplate":
                    return requestContainer.Resolve<ICassetteRequestHandler>("HtmlTemplateBundleRequestHandler");

                case "asset":
                    return requestContainer.Resolve<ICassetteRequestHandler>("AssetRequestHandler");
                    
                default:
                    throw new HttpException(404, "Resource not found.");
            }
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            ProcessRequest();
        }

        bool IHttpHandler.IsReusable
        {
            get { return false; }
        }
    }
}