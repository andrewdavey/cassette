using System;
using System.Web;
using System.Web.Caching;

namespace Cassette.Web
{
    public class CassetteHttpModule : IHttpModule
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
                "Cassette.Manager",
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
                    CassetteHttpModule.manager = new Lazy<IManager>(CreateManager);
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
                
                var placeholderTracker = MaybeCreatePlaceholderTracker();

                var pageHelper = CreatePageHelper(httpContext, placeholderTracker, Manager);
                StorePageHelperInHttpContextItems(pageHelper, httpContext);
                if (Manager.Configuration.BufferHtmlOutput)
                {
                    InstallResponseFilter(placeholderTracker, httpContext);
                }
            };
        }

        IPlaceholderTracker MaybeCreatePlaceholderTracker()
        {
            if (Manager.Configuration.BufferHtmlOutput)
            {
                return new PlaceholderTracker();
            }
            return null;
        }

        void StorePageHelperInHttpContextItems(IPageHelper pageHelper, HttpContextBase httpContext)
        {
            httpContext.Items["Cassette.PageHelper"] = pageHelper;
        }

        void InstallResponseFilter(IPlaceholderTracker placeholderTracker, HttpContextBase context)
        {
            context.Response.Filter = new BufferStream(context.Response.Filter, context, placeholderTracker);
        }

        static PageHelper CreatePageHelper(HttpContextBase httpContext, IPlaceholderTracker placeholderTracker, IManager manager)
        {
            var scriptReferenceBuilder = new ReferenceBuilder(manager.ScriptModuleContainer);
            var stylesheetReferenceBuilder = new ReferenceBuilder(manager.StylesheetModuleContainer);
            var htmlTemplateReferenceBuilder = new ReferenceBuilder(manager.HtmlTemplateModuleContainer);
            return new PageHelper(
                httpContext,
                placeholderTracker,
                manager.Configuration,
                scriptReferenceBuilder,
                stylesheetReferenceBuilder,
                htmlTemplateReferenceBuilder
            );
        }

        public static IPageHelper GetPageHelper(HttpContextBase httpContext = null)
        {
            if (httpContext == null) httpContext = new HttpContextWrapper(HttpContext.Current);

            var helper = httpContext.Items["Cassette.PageHelper"] as IPageHelper;
            if (helper == null)
            {
                throw new InvalidOperationException("Cassette.PageHelper has not been added to the current HttpContext Items. Make sure the CassetteHttpModule has been added to Web.config.");
            }
            return helper;
        }

        public void Dispose()
        {
        }
    }
}
