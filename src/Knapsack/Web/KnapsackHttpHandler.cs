using System.Web;

namespace Knapsack.Web
{
    public class KnapsackHttpHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var modulePath = context.Request.PathInfo.Substring(1);
            var module = KnapsackHttpModule.Instance.ModuleContainer.FindModule(modulePath);
            
            if (module == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            var serverETag = module.Hash.ToHexString();
            var clientETag = context.Request.Headers["If-None-Match"];
            if (clientETag == serverETag)
            {
                context.Response.StatusCode = 304; // Not Modified;
            }
            else
            {
                context.Response.ContentType = "text/javascript";
                context.Response.AddHeader("ETag", serverETag);
                using (var stream = KnapsackHttpModule.Instance.Cache.OpenModuleFile(module))
                {
                    stream.CopyTo(context.Response.OutputStream);
                }
            }
        }
    }
}
