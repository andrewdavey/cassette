using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Routing;
using Cassette.UI;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: WebActivator.PreApplicationStartMethod(
    typeof(Cassette.Web.StartUp),
    "PreApplicationStart"
)]
[assembly: WebActivator.PostApplicationStartMethod(
    typeof(Cassette.Web.StartUp), 
    "PostApplicationStart"
)]
[assembly: WebActivator.ApplicationShutdownMethod(
    typeof(Cassette.Web.StartUp),
    "ApplicationShutdown"
)]

namespace Cassette.Web
{
    public static class StartUp
    {
        static ICassetteConfiguration configuration;
        static IsolatedStorageFile storage;
        static FileSystemWatcher watcher;
        static Lazy<CassetteApplication> cassetteApplication;

        public static CassetteApplication CassetteApplication
        {
            get { return cassetteApplication.Value; }
        }

        // If using an IoC container, this delegate can be replaced (in Application_Start) to
        // provide an alternative way to create the configuration object.
        public static Func<ICassetteConfiguration> CreateConfiguration = CreateConfigurationByScanningAssembliesForType;

        public static void PreApplicationStart()
        {
            DynamicModuleUtility.RegisterModule(typeof(CassetteHttpModule));
        }

        // This runs *after* Global.asax Application_Start.
        public static void PostApplicationStart()
        {
            storage = IsolatedStorageFile.GetMachineStoreForAssembly();

            configuration = CreateConfiguration();
            cassetteApplication = new Lazy<CassetteApplication>(CreateCassetteApplication);
            
            Assets.GetApplication = () => CassetteApplication;

            if (ShouldOptimizeOutput() == false)
            {
                ReloadApplicationOnFileSystemChange();
            }
        }

        static void ReloadApplicationOnFileSystemChange()
        {
            watcher = new FileSystemWatcher(CassetteApplication.RootDirectory.GetAbsolutePath(""))
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            watcher.Changed += InvalidateCassetteApplication;
            watcher.Renamed += InvalidateCassetteApplication;
            watcher.Deleted += InvalidateCassetteApplication;
        }

        static void InvalidateCassetteApplication(object sender, FileSystemEventArgs e)
        {
            if (cassetteApplication.IsValueCreated)
            {
                cassetteApplication = new Lazy<CassetteApplication>(CreateCassetteApplication);
            }
        }

        public static void ApplicationShutdown()
        {
            if (watcher != null) watcher.Dispose();
            storage.Dispose();
            CassetteApplication.Dispose();
        }

        static ICassetteConfiguration CreateConfigurationByScanningAssembliesForType()
        {
            // Scan all assemblies for implementation of the interface and create instance.
            var types = from filename in Directory.GetFiles(HttpRuntime.BinDirectory, "*.dll")
                        let assembly = Assembly.LoadFrom(filename)
                        from type in assembly.GetExportedTypes()
                        where type.IsClass
                           && !type.IsAbstract
                           && type != typeof(EmptyCassetteConfiguration)
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

        static CassetteApplication CreateCassetteApplication()
        {
            return new CassetteApplication(
                configuration,
                new FileSystem(HttpRuntime.AppDomainAppPath),
                new IsolatedStorageFileSystem(storage),
                ShouldOptimizeOutput(),
                GetConfigurationVersion(configuration, HttpRuntime.AppDomainAppVirtualPath),
                new UrlGenerator(HttpRuntime.AppDomainAppVirtualPath),
                RouteTable.Routes,
                GetCurrentHttpContext
            );
        }

        static bool ShouldOptimizeOutput()
        {
            var compilation = WebConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
            return (compilation != null && compilation.Debug) == false;
        }

        static string GetConfigurationVersion(ICassetteConfiguration configuration, string virtualDirectory)
        {
            var assemblyVersion = configuration.GetType()
                .Assembly
                .GetName()
                .Version
                .ToString();
            return assemblyVersion + "|" + virtualDirectory;
        }

        static HttpContextBase GetCurrentHttpContext()
        {
            return new HttpContextWrapper(HttpContext.Current);
        }
    }
}
