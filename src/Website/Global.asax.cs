using System.Web.Mvc;
using System.Web.Routing;

namespace Website
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Home", "", new { controller = "Home", action = "Index" });
            routes.MapRoute("Benefits", "benefits", new { controller = "Home", action = "Benefits" });
            routes.MapRoute("Download", "download", new { controller = "Home", action = "Download" });
            routes.MapRoute("FAQs", "faqs", new { controller = "Home", action = "FAQs" });
            routes.MapRoute("Support", "support", new { controller = "Home", action = "Support" });
            routes.MapRoute("Contact", "contact", new { controller = "Home", action = "Contact" });
            routes.MapRoute("Donate", "donate", new { controller = "Home", action = "Donate" });
            routes.MapRoute(
                "Documentation", 
                "documentation/{version}/{*path}",
                new { controller = "Documentation", action = "Index", path = "" },
                new { version = @"v\d+" }
            );
            routes.MapRoute("OldDocumentation", "documentation/{*path}", new { controller = "Documentation", action = "OldIndex", path = "" });
            routes.MapRoute("Licensing", "licensing", new { controller = "Home", action = "Licensing" });
            routes.MapRoute("Resources", "resources", new { controller = "Home", action = "Resources" });

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}
