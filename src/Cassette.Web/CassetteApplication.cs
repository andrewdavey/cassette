using System.Web.Routing;
using Cassette.UI;
using System.Web;

namespace Cassette.Web
{
    public class CassetteApplication : Cassette.CassetteApplicationBase
    {
        public CassetteApplication(IFileSystem sourceFileSystem, IFileSystem cacheFileSystem, UrlGenerator urlGenerator, bool isOutputOptmized, string version)
            : base(sourceFileSystem, cacheFileSystem, isOutputOptmized, version)
        {
            this.urlGenerator = urlGenerator;
        }

        readonly UrlGenerator urlGenerator;

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
            var key = typeof(IPageAssetManager<T>).FullName;
            if (HttpContext.Current.Items.Contains(key))
            {
                return (IPageAssetManager<T>)HttpContext.Current.Items[key];
            }
            else
            {
                var manager = new PageAssetManager<T>(new ReferenceBuilder<T>(GetModuleContainer<T>()), this);
                HttpContext.Current.Items[key] = manager;
                return manager;
            }
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
