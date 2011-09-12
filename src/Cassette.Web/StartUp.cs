using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Configuration;
using System.Web.Routing;
using Cassette.IO;
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
        static IEnumerable<ICassetteConfiguration> configurations;
        static IsolatedStorageFile storage;
        static CassetteApplicationContainer<CassetteApplication> applicationContainer;
 
        public static CassetteApplication CassetteApplication
        {
            get { return applicationContainer.Application; }
        }

        // If using an IoC container, this delegate can be replaced (in Application_Start) to
        // provide an alternative way to create the configuration object.
        public static Func<IEnumerable<ICassetteConfiguration>> CreateConfigurations = CreateConfigurationsByScanningAssembliesForType;

        public static void PreApplicationStart()
        {
            DynamicModuleUtility.RegisterModule(typeof(CassetteHttpModule));
        }

        // This runs *after* Global.asax Application_Start.
        public static void PostApplicationStart()
        {
            storage = IsolatedStorageFile.GetMachineStoreForAssembly();
            
            configurations = CreateConfigurations();
            applicationContainer = ShouldOptimizeOutput() ? new CassetteApplicationContainer<CassetteApplication>(CreateCassetteApplication) 
                                                          : new CassetteApplicationContainer<CassetteApplication>(CreateCassetteApplication, HttpRuntime.AppDomainAppPath);
            
            Assets.GetApplication = () => CassetteApplication;
        }

        public static void ApplicationShutdown()
        {
            storage.Dispose();
            applicationContainer.Dispose();
        }

        static IEnumerable<ICassetteConfiguration> CreateConfigurationsByScanningAssembliesForType()
        {
            // Scan all assemblies for implementation of the interface and create instance.
            return from assembly in LoadAllAssemblies()
                   from type in assembly.GetExportedTypes()
                   where type.IsClass
                      && !type.IsAbstract
                      && typeof(ICassetteConfiguration).IsAssignableFrom(type)
                   select (ICassetteConfiguration)Activator.CreateInstance(type);
        }

        static IEnumerable<Assembly> LoadAllAssemblies()
        {
            const int COR_E_ASSEMBLYEXPECTED = -2146234344;
            foreach (var filename in Directory.GetFiles(HttpRuntime.BinDirectory, "*.dll"))
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(filename);
                }
                catch (BadImageFormatException exception)
                {
                    if (Marshal.GetHRForException(exception) == COR_E_ASSEMBLYEXPECTED) // Was not a managed DLL.
                    {
                        continue;
                    }
                    throw;
                }
                yield return assembly;
            }
        }

        static CassetteApplication CreateCassetteApplication()
        {
            return new CassetteApplication(
                configurations,
                new FileSystemDirectory(HttpRuntime.AppDomainAppPath),
                GetCacheDirectory(),
                ShouldOptimizeOutput(),
                GetConfigurationVersion(HttpRuntime.AppDomainAppVirtualPath),
                new UrlGenerator(HttpRuntime.AppDomainAppVirtualPath),
                RouteTable.Routes,
                GetCurrentHttpContext
            );
        }

        static IDirectory GetCacheDirectory()
        {
            return new IsolatedStorageDirectory(storage);
            // TODO: Add configuration setting to use App_Data
            //return new FileSystem(Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data", ".CassetteCache"));
        }

        static bool ShouldOptimizeOutput()
        {
            var compilation = WebConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
            return (compilation != null && compilation.Debug) == false;
        }

        static string GetConfigurationVersion(string virtualDirectory)
        {
            var assemblyVersion = configurations.Select(
                configuration => new AssemblyName(configuration.GetType().Assembly.FullName).Version.ToString()
            ).Distinct();

            var parts = assemblyVersion.Concat(new[] { virtualDirectory });
            return string.Join("|", parts);
        }

        static HttpContextBase GetCurrentHttpContext()
        {
            return new HttpContextWrapper(HttpContext.Current);
        }
    }
}
