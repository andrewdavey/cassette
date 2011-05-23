using System;
using System.Web;
using System.Web.Caching;

namespace Knapsack.Web
{
    public class KnapsackHttpModule : IHttpModule
    {
        // Using a static Lazy<T> means we get a singleton Manager which is created in a thread-safe manner.
        static Lazy<IManager> manager = new Lazy<IManager>(CreateManager);

        static IManager CreateManager()
        {
            IManager manager;
            try
            {
                manager = new Manager();
            }
            catch (Exception ex)
            {
                // We don't want to throw the exception during lazy initialization because
                // there is no chance to recreate the object, even if, say, a broken file is fixed.
                // Instead, we cache the exception in this special implementation of IManager
                // and throw it when trying to call members. The CreateCacheDependency()
                // method creates a special dependency that will be triggered just before 
                // the exception is thrown. This provides us with a chance to re-create the
                // lazy object with fixed (we hope!) files.
                manager = new ExceptionCachedManager(ex);
            }
            CacheManagerWithDependency(manager);
            return manager;
        }

        static void CacheManagerWithDependency(IManager manager)
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
                    IDisposable disposable = value as IDisposable;
                    if (disposable != null) disposable.Dispose();
                    // Manager is removed from cache when the file system is changed.
                    // So we clear the old instance by reassigning the lazy object.
                    // It'll be recreated next time someone requests it.
                    KnapsackHttpModule.manager = new Lazy<IManager>(CreateManager);
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
                var httpContext = new HttpContextWrapper(HttpContext.Current);
                var pageHelper = CreatePageHelper(httpContext);
                StorePageHelperInHttpContextItems(pageHelper, httpContext);
                if (Manager.Configuration.BufferHtmlOutput)
                {
                    InstallResponseFilter(pageHelper, httpContext);
                }
            };
        }

        void StorePageHelperInHttpContextItems(IPageHelper pageHelper, HttpContextBase httpContext)
        {
            httpContext.Items["Knapsack.PageHelper"] = pageHelper;
        }

        void InstallResponseFilter(IPageHelper pageHelper, HttpContextBase context)
        {
            context.Response.Filter = new BufferStream(context.Response.Filter, context, pageHelper);
        }

        static PageHelper CreatePageHelper(HttpContextBase httpContext)
        {
            var scriptReferenceBuilder = new ReferenceBuilder(Manager.ScriptModuleContainer);
            var stylesheetReferenceBuilder = new ReferenceBuilder(Manager.StylesheetModuleContainer);
            var useModules = Manager.Configuration.ShouldUseModules(httpContext);
            return new PageHelper(useModules, Manager.Configuration.BufferHtmlOutput, scriptReferenceBuilder, stylesheetReferenceBuilder, VirtualPathUtility.ToAbsolute);
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
