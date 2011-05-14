using System;
using System.Web;

namespace Knapsack.Web
{
    public class KnapsackHttpModule : IHttpModule
    {
        // Using a static Lazy<T> means we get a singleton Manager which is created in a thread-safe manner.
        static Lazy<Manager> manager = new Lazy<Manager>();
        
        public static IManager Manager
        {
            get 
            {
                return manager.Value;
            }
        }

        public void Init(HttpApplication application)
        {
            application.BeginRequest += (sender, e) =>
            {
                StorePageHelperInHttpContextItems();
            };
        }

        void StorePageHelperInHttpContextItems()
        {
            var httpContext = new HttpContextWrapper(HttpContext.Current);
            httpContext.Items["Knapsack.PageHelper"] = CreatePageHelper(httpContext);
        }

        static PageHelper CreatePageHelper(HttpContextBase httpContext)
        {
            var scriptReferenceBuilder = new ReferenceBuilder(Manager.ScriptModuleContainer);
            var stylesheetReferenceBuilder = new ReferenceBuilder(Manager.StylesheetModuleContainer);
            var useModules = Manager.Configuration.ShouldUseModules(httpContext);
            return new PageHelper(useModules, scriptReferenceBuilder, stylesheetReferenceBuilder, VirtualPathUtility.ToAbsolute);
        }

        public static IPageHelper GetPageHelper(HttpContextBase httpContext)
        {
            var helper = httpContext.Items["Knapsack.PageHelper"] as IPageHelper;
            if (helper == null)
            {
                throw new InvalidOperationException("Knapsack.PageHelper has not been added to the current HttpContext Items. Make sure the KnapsackHttpModule has been added to Web.config.");
            }
            return helper;
        }

        public void Dispose()
        {
        }
    }
}
