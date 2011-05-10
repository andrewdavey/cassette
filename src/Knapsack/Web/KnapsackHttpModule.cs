using System;
using System.Web;

namespace Knapsack.Web
{
    public class KnapsackHttpModule : IHttpModule
    {
        // Using Lazy<T> means we get a singleton Manager which is created in a thread-safe manner.
        static Lazy<IManager> manager = new Lazy<IManager>();
        
        public static IManager Manager
        {
            get 
            {
                return manager.Value;
            }
            internal set // for unit tests
            {
                manager = new Lazy<IManager>(() => value);
            }
        }

        public void Init(HttpApplication application)
        {
            application.BeginRequest += HandleBeginRequest;
        }

        void HandleBeginRequest(object sender, EventArgs e)
        {
            StorePageHelperInHttpContextItems();
        }

        void StorePageHelperInHttpContextItems()
        {
            var httpContext = new HttpContextWrapper(HttpContext.Current);
            httpContext.Items["Knapsack.PageHelper"] = CreatePageHelper(httpContext);
        }

        static PageHelper CreatePageHelper(HttpContextBase httpContext)
        {
            var referenceBuilder = new ReferenceBuilder(Manager.ModuleContainer);
            var useModules = Manager.Configuration.ShouldUseModules(httpContext);
            return new PageHelper(useModules, referenceBuilder, VirtualPathUtility.ToAbsolute);
        }

        public void Dispose()
        {
        }
    }
}
