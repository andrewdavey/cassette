using System.Web.Routing;
using Cassette.UI;
using System.Web;
using System.Collections.Generic;
using Cassette.CoffeeScript;
using Cassette.Less;

namespace Cassette.Web
{
    public class CassetteApplication : Cassette.CassetteApplicationBase
    {
        public CassetteApplication(IFileSystem sourceFileSystem, IFileSystem cacheFileSystem, UrlGenerator urlGenerator, bool isOutputOptmized, string version)
            : base(sourceFileSystem, cacheFileSystem, isOutputOptmized, version)
        {
            this.urlGenerator = urlGenerator;
            AddCompiler("coffee", new CoffeeScriptCompiler());
            AddCompiler("less", new LessCompiler());
        }

        readonly UrlGenerator urlGenerator;
        readonly Dictionary<string, ICompiler> compilers = new Dictionary<string, ICompiler>();
        static readonly string PlaceholderTrackerKey = typeof(IPlaceholderTracker).FullName;

        public void AddCompiler(string fileExtension, ICompiler compiler)
        {
            compilers.Add(fileExtension, compiler);
        }

        public override string CreateAssetUrl(Module module, IAsset asset)
        {
            return urlGenerator.CreateAssetUrl(module, asset);
        }

        public override string CreateModuleUrl(Module module)
        {
            return urlGenerator.CreateModuleUrl(module);
        }

        public override IPageAssetManager<T> GetPageAssetManager<T>()
        {
            var items = HttpContext.Current.Items;
            var key = typeof(IPageAssetManager<T>).FullName;
            if (items.Contains(key))
            {
                return (IPageAssetManager<T>)items[key];
            }
            else
            {
                var manager = new PageAssetManager<T>(
                    new ReferenceBuilder<T>(GetModuleContainer<T>()), 
                    this,
                    (IPlaceholderTracker)items[PlaceholderTrackerKey]
                );
                items[key] = manager;
                return manager;
            }
        }

        public void OnBeginRequest(HttpContextBase httpContext)
        {
            var tracker = new PlaceholderTracker();
            httpContext.Items[PlaceholderTrackerKey] = tracker;

            var response = httpContext.Response;
            response.Filter = new PlaceholderReplacingResponseFilter(
                response,
                tracker
            );
        }

        public void InstallRoutes(RouteCollection routes)
        {
            // Insert Cassette's routes at the start of the table, 
            // to avoid conflicts with the application's own routes.
            InstallModuleRoute<ScriptModule>();
            InstallModuleRoute<StylesheetModule>();
            InstallModuleRoute<HtmlTemplateModule>();

            RouteTable.Routes.Insert(0, new Route("_assets/compile/{*path}", null)); //coffee, less, etc
        }

        void InstallModuleRoute<T>()
            where T : Module
        {
            var url = urlGenerator.ModuleUrlPattern<T>();
            var handler = new ModuleRouteHandler<T>(GetModuleContainer<T>());
            RouteTable.Routes.Insert(0, new Route(url, handler));
        }
    }
}
