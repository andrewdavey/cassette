using System.Web.Routing;
using TinyIoC;

namespace Cassette.Web.Jasmine
{
    public class StartUp : IStartUpTask
    {
        readonly RouteCollection routes;
        readonly TinyIoCContainer container;

        public StartUp(RouteCollection routes, TinyIoCContainer container)
        {
            this.routes = routes;
            this.container = container;
        }

        public void Run()
        {
            InstallRoute();
        }

        void InstallRoute()
        {
            var route = new CassetteRoute(
                "_cassette/jasmine/{*specbundle}",
                new CassetteRouteHandler<PageHandler>(container)
            );
            routes.Insert(0, route);
        }
    }
}