using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DotNet35
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Color",
                "colors/{id}",
                new { controller = "Color", action = "item" }
            );
            routes.MapRoute(
                "Colors",
                "colors",
                new { controller = "Color", action = "list" }
            );
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            // Start up Cassette
            Cassette.Web.StartUp.PreApplicationStart();

            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);

            // Occurs last
            Cassette.Web.StartUp.PostApplicationStart();
        }

        protected void Application_End()
        {
            // Handle shutdown
            Cassette.Web.StartUp.ApplicationShutdown();
        }
    }
}