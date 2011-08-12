using System;
using System.IO;
using System.Web;
using System.Web.Routing;
using Cassette.Compilation;

namespace Cassette.Web
{
    public class CompileRequestHandler : IHttpHandler
    {
        public CompileRequestHandler(RequestContext requestContext, Func<string, ICompiler> getCompiler)
        {
            this.requestContext = requestContext;
            this.getCompiler = getCompiler;
        }

        readonly RequestContext requestContext;
        readonly Func<string, ICompiler> getCompiler;

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext _)
        {
            var path = requestContext.RouteData.GetRequiredString("path");
            var response = requestContext.HttpContext.Response;
            var filename = requestContext.HttpContext.Server.MapPath("~/" + path);
            if (File.Exists(filename) == false)
            {
                response.StatusCode = 404;
                response.End();
                return;
            }

            var extension = Path.GetExtension(path).Substring(1);
            var compiler = getCompiler(extension);
            var source = File.ReadAllText(filename);
            var output = compiler.Compile(
                source, 
                Path.GetFileName(filename), 
                new FileSystem(Path.GetDirectoryName(filename))
            );

            response.ContentType = compiler.OutputContentType;
            response.Write(output);
        }
    }
}
