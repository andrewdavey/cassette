using System.Web.Routing;
using StartUp = Cassette.Web.Jasmine.StartUp;

[assembly: WebActivator.PostApplicationStartMethod(typeof(StartUp), "PostApplicationStart")]

namespace Cassette.Web.Jasmine
{
    public static class StartUp
    {
        public static void PostApplicationStart()
        {
            InstallRoute(RouteTable.Routes);
        }

        static void InstallRoute(RouteCollection routes)
        {
            var route = new CassetteRoute(
                "_cassette/jasmine/{*specbundle}",
                new DelegateRouteHandler(context => new PageHandler(context))
            );
            routes.Insert(0, route);
        }
    }
}