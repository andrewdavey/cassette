using System;
using System.Web;
using System.Web.Caching;

namespace Knapsack.Web
{
    public class KnapsackHttpModule : IHttpModule
    {
        // Using a static Lazy<T> means we get a singleton Manager which is created in a thread-safe manner.
        static Lazy<Manager> manager = new Lazy<Manager>(CreateManager);

        static Manager CreateManager()
        {
            var manager = new Manager();
            CacheManagerWithDependency(manager);
            return manager;
        }

        static void CacheManagerWithDependency(Manager manager)
        {
            var dependency = manager.CreateCacheDependency();
            HttpRuntime.Cache.Insert(
                "Knapsack.Manager",
                manager,
                dependency,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.Normal,
                (key, value, reason) =>
                {
                    ((Manager)value).Dispose();
                    // Manager is removed from cache when the file system is changed.
                    // So we clear the old instance by reassigning the lazy object.
                    // It'll be recreated next time someone requests it.
                    KnapsackHttpModule.manager = new Lazy<Manager>(CreateManager);
                }
            );
        }

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
            var referenceBuilder = new ReferenceBuilder(Manager.ModuleContainer);
            var useModules = Manager.Configuration.ShouldUseModules(httpContext);
            return new PageHelper(useModules, referenceBuilder, VirtualPathUtility.ToAbsolute);
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
