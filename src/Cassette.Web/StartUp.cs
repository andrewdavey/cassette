using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using System.IO.IsolatedStorage;

[assembly: WebActivator.PostApplicationStartMethod(
    typeof(Cassette.Web.StartUp), 
    "PostApplicationStart"
)]
[asembly: WebActivator.ApplicationShutdownMethod(
    typeof(Cassette.Web.StartUp),
    "ApplicationShutdown"
)]

namespace Cassette.Web
{
    public static class StartUp
    {
        static IsolatedStorageFile storage;
        static IFileSystem cacheFileSystem;
        public static CassetteApplication CassetteApplication { get; private set; }

        public static Func<ICassetteConfiguration> CreateConfiguration = CreateConfigurationByScanningAssembliesForType;

        // This runs *after* Global.asax Application_Start.
        public static void PostApplicationStart()
        {
            storage = IsolatedStorageFile.GetMachineStoreForAssembly();
            var configuration = CreateConfiguration();
            CassetteApplication = CreateCassetteApplication(configuration);
            CassetteApplication.InitializeModuleContainers();
            InstallRoutes();
        }

        public static void ApplicationShutdown()
        {
            storage.Dispose();
        }

        static ICassetteConfiguration CreateConfigurationByScanningAssembliesForType()
        {
            // Scan all assemblies for implementation of the interface and create instance.
            var types = from filename in Directory.GetFiles(HttpRuntime.BinDirectory, "*.dll")
                        let assembly = Assembly.LoadFrom(filename)
                        from type in assembly.GetExportedTypes()
                        where type.IsClass
                           && !type.IsAbstract
                           && typeof(ICassetteConfiguration).IsAssignableFrom(type)
                        select type;

            var configType = types.FirstOrDefault();
            if (configType == null)
            {
                // No configuration defined. Any attempt to get asset modules later will fail with 
                // an exception. The exception message will tell the developer to create a configuration class.
                return new EmptyCassetteConfiguration();
            }
            else
            {
                return (ICassetteConfiguration)Activator.CreateInstance(configType);
            }
        }

        static CassetteApplication CreateCassetteApplication(ICassetteConfiguration configuration)
        {
            var application = new CassetteApplication(
                HttpRuntime.AppDomainAppPath, 
                HttpRuntime.AppDomainAppVirtualPath,
                new IsolatedStorageFileSystem(storage)
            );
            configuration.Configure(application);
            return application;
        }

        static void InstallRoutes()
        {
            // Insert Cassette's routes at the start of the table, 
            // to avoid conflicts with the application's own routes.
            RouteTable.Routes.Insert(0, new Route("_assets/module/{*path}", null));
            RouteTable.Routes.Insert(0, new Route("_assets/compile/{*path}", null)); //coffee, less, etc
        }
    }
}
