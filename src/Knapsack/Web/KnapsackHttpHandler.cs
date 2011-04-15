using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var modulePath = context.Request.PathInfo.Substring(1).Replace('/','\\');
            var module = KnapsackHttpModule.Instance.ModuleContainer.FindModule(modulePath);
            if (module != null)
            {
                context.Response.ContentType = "text/javascript";
                using (var stream = KnapsackHttpModule.Instance.Cache.OpenModuleFile(module))
                {
                    stream.CopyTo(context.Response.OutputStream);
                }
            }
            else
            {
                context.Response.StatusCode = 404;
            }
        }
    }
}
