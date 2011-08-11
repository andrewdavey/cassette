using System.Web.Routing;
using Cassette.UI;

namespace Cassette.Web
{
    public class CassetteApplication : Cassette.CassetteApplicationBase
    {
        public CassetteApplication(IFileSystem sourceFileSystem, IFileSystem cacheFileSystem, UrlGenerator urlGenerator, bool isOutputOptmized)
            : base(sourceFileSystem, cacheFileSystem, isOutputOptmized)
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
            return new PageAssetManager<T>(new ReferenceBuilder<T>(GetModuleContainer<T>()), this);
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
