using System.IO.IsolatedStorage;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using Cassette.Configuration;

namespace Cassette.Web
{
    /// <summary>
    /// A single Manager object is created for the web application and contains all the top-level
    /// objects used by Cassette.
    /// </summary>
    public class CassetteApplication : CassetteApplicationBase, ICassetteApplication
    {
        public CassetteApplication()
            : base(
                GetConfiguration(), 
                HttpRuntime.AppDomainAppPath, 
                HttpRuntime.AppDomainAppVirtualPath, 
                IsolatedStorageFile.GetMachineStoreForDomain()
            )
        {
        }

        public IPageAssetManager CreatePageAssetManager(HttpContextBase httpContext)
        {
            var useModules = ShouldUseModules(httpContext);
            if (configuration.BufferHtmlOutput)
            {
                var placeholderTracker = new PlaceholderTracker();
                InstallResponseFilter(placeholderTracker, httpContext);
                return CreatePageAssetManager(useModules, placeholderTracker);
            }
            else
            {
                return CreatePageAssetManager(useModules, null);
            }
        }

        public IHttpHandler CreateHttpHandler()
        {
            return new CassetteHttpHandler(
                () => scriptModuleContainer,
                () => stylesheetModuleContainer,
                coffeeScriptCompiler,
                lessCompiler
            );
        }

        /// <summary>
        /// Returns a CacheDependency object that watches all module source directories for changes.
        /// </summary>
        public CacheDependency CreateCacheDependency()
        {
            var scripts = GetDirectoriesToWatch(configuration.Scripts, "scripts");
            var styles = GetDirectoriesToWatch(configuration.Styles, "styles");
            var paths = scripts.Concat(styles).ToArray();
            return new CacheDependency(paths);
        }

        bool ShouldUseModules(HttpContextBase context)
        {
            return configuration.ModuleMode == ModuleMode.On
                || (configuration.ModuleMode == ModuleMode.OffInDebug
                    && !context.IsDebuggingEnabled);
        }

        static CassetteSection GetConfiguration()
        {
            return (CassetteSection)WebConfigurationManager.GetSection("cassette");
        }

        void InstallResponseFilter(IPlaceholderTracker placeholderTracker, HttpContextBase context)
        {
            context.Response.Filter = new PlaceholderReplacingResponseFilter(context.Response, placeholderTracker);
        }
    }
}
