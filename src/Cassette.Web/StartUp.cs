#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Routing;
using Cassette.Configuration;
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
 
        internal static CassetteApplication CassetteApplication
        {
            get { return applicationContainer.Application; }
        }

        // If using an IoC container, this delegate can be replaced (in Application_Start) to
        // provide an alternative way to create the configuration object.
        public static Func<IEnumerable<ICassetteConfiguration>> CreateConfigurations = CreateConfigurationsByScanningAssembliesForType;

        public static void PreApplicationStart()
        {
            Trace.Source.TraceInformation("Registering CassetteHttpModule.");
            DynamicModuleUtility.RegisterModule(typeof(CassetteHttpModule));
        }

        // This runs *after* Global.asax Application_Start.
        public static void PostApplicationStart()
        {
            storage = IsolatedStorageFile.GetMachineStoreForAssembly(); // TODO: Check if this should be GetMachineStoreForApplication instead
            
            configurations = CreateConfigurations();
            applicationContainer = ShouldOptimizeOutput() ? new CassetteApplicationContainer<CassetteApplication>(CreateCassetteApplication) 
                                                          : new CassetteApplicationContainer<CassetteApplication>(CreateCassetteApplication, HttpRuntime.AppDomainAppPath);
            
            Assets.GetApplication = () => CassetteApplication;
        }

        public static void ApplicationShutdown()
        {
            Trace.Source.TraceInformation("Application shutdown - disposing resources.");
            storage.Dispose();
            applicationContainer.Dispose();
        }

        static IEnumerable<ICassetteConfiguration> CreateConfigurationsByScanningAssembliesForType()
        {
            Trace.Source.TraceInformation("Creating CassetteConfigurations by scanning assemblies.");
            // Scan all assemblies for implementation of the interface and create instance.
            return from assembly in BuildManager.GetReferencedAssemblies().Cast<Assembly>()
                   from type in assembly.GetExportedTypes()
                   where type.IsClass
                      && !type.IsAbstract
                      && typeof(ICassetteConfiguration).IsAssignableFrom(type)
                   select CreateConfigurationInstance(type);
        }

        static ICassetteConfiguration CreateConfigurationInstance(Type type)
        {
            Trace.Source.TraceInformation("Creating {0}.", type.FullName);

            return (ICassetteConfiguration)Activator.CreateInstance(type);
        }

        static CassetteApplication CreateCassetteApplication()
        {
            Trace.Source.TraceInformation("Create Cassette application object.");
            
            var sourceDirectory = HttpRuntime.AppDomainAppPath;
            Trace.Source.TraceInformation("Source directory: {0}", sourceDirectory);
            
            var isOutputOptmized = ShouldOptimizeOutput();
            Trace.Source.TraceInformation("IsOutputOptimized: {0}", isOutputOptmized);

            var version = GetConfigurationVersion(HttpRuntime.AppDomainAppVirtualPath);
            Trace.Source.TraceInformation("Cache version: {0}", version);

            var configurable = new ConfigurableCassetteApplication
            {
                Settings =
                {
                    IsDebuggingEnabled = isOutputOptmized == false,
                    IsHtmlRewritingEnabled = true,
                    SourceDirectory = new FileSystemDirectory(sourceDirectory),
                    CacheDirectory = GetCacheDirectory()
                }
            };
            foreach (var configuration in new[] { new InitialConfiguration(sourceDirectory, storage) }.Concat(configurations))
            {
                configuration.Configure(configurable);
            }
            
            var urlModifier = configurable.Services.CreateUrlModifier();
            var routing = new CassetteRouting(urlModifier);

            return new CassetteApplication(
                configurable,
                version,
                routing,
                RouteTable.Routes,
                GetCurrentHttpContext
            );
        }

        /// <remarks>
        /// We need bundle container cache to depend on both the application version
        /// and the Cassette version. So if either is upgraded, then the cache is discarded.
        /// </remarks>
        static string CombineVersionWithCassetteVersion(string version)
        {
            return version + "|" + typeof(ICassetteApplication).Assembly.GetName().Version;
        }

        static IDirectory GetCacheDirectory()
        {
            Trace.Source.TraceInformation("Using isolated storage for cache.");
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

