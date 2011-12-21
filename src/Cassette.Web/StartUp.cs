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
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Routing;
using Cassette.Configuration;
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
    /// <summary>
    /// Controls the lifetime of the Cassette infrastructure by handling application startup and shutdown.
    /// </summary>
    public static class StartUp
    {
        static IEnumerable<ICassetteConfiguration> _configurations;
        static IsolatedStorageFile _storage;
        static Stopwatch _startupTimer;
        static readonly object CreationLock = new object();
        /// <summary>
        /// Collects all of Cassette's trace output during start-up.
        /// </summary>
        static readonly TraceListener StartupTraceListener = new StringBuilderTraceListener
        {
            TraceOutputOptions = TraceOptions.DateTime,
            Filter = new EventTypeFilter(SourceLevels.All)
        };

        /// <summary>
        /// The function delegate used to create Cassette configuration objects for the application.
        /// By default this will scan the AppDomain's assemblies for all implementations of <see cref="ICassetteConfiguration"/>.
        /// Assignment to this property should happen in Application_Start.
        /// </summary>
// ReSharper disable MemberCanBePrivate.Global
        public static Func<IEnumerable<ICassetteConfiguration>> CreateConfigurations { get; set; }
// ReSharper restore MemberCanBePrivate.Global

        static StartUp()
        {
            CreateConfigurations = CreateConfigurationsByScanningAssembliesForType;
        }

// ReSharper disable UnusedMember.Global
        public static void PreApplicationStart()
// ReSharper restore UnusedMember.Global
        {
            Trace.Source.Listeners.Add(StartupTraceListener);

            _startupTimer = Stopwatch.StartNew();
            Trace.Source.TraceInformation("Registering CassetteHttpModule.");
            DynamicModuleUtility.RegisterModule(typeof(CassetteHttpModule));
        }

        // This runs *after* Global.asax Application_Start.
// ReSharper disable UnusedMember.Global
        public static void PostApplicationStart()
// ReSharper restore UnusedMember.Global
        {
            try
            {
                Trace.Source.TraceInformation("PostApplicationStart.");

                _storage = IsolatedStorageFile.GetMachineStoreForAssembly();
                // TODO: Check if this should be GetMachineStoreForApplication instead

                _configurations = CreateConfigurations();

                var container = CreateCassetteApplicationContainer();
                CassetteApplicationContainer.Instance = container;
                container.ForceApplicationCreation();

                Trace.Source.TraceInformation("Cassette startup completed. It took {0} ms.", _startupTimer.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                Trace.Source.TraceEvent(TraceEventType.Error, 0, ex.Message);
                throw;
            }
            finally
            {
                _startupTimer.Stop();
                Trace.Source.Flush();
                Trace.Source.Listeners.Remove(StartupTraceListener);
            }
        }

// ReSharper disable UnusedMember.Global
        public static void ApplicationShutdown()
// ReSharper restore UnusedMember.Global
        {
            Trace.Source.TraceInformation("Application shutdown - disposing resources.");
            _storage.Dispose();
            CassetteApplicationContainer.Instance.Dispose();
        }

        static CassetteApplicationContainer CreateCassetteApplicationContainer()
        {
            return GetSystemWebCompilationDebug()
                       ? new CassetteApplicationContainer(CreateCassetteApplication, HttpRuntime.AppDomainAppPath)
                       : new CassetteApplicationContainer(CreateCassetteApplication);
        }

        static internal string TraceOutput
        {
            get { return StartupTraceListener.ToString(); }
        }

        static IEnumerable<ICassetteConfiguration> CreateConfigurationsByScanningAssembliesForType()
        {
            Trace.Source.TraceInformation("Creating CassetteConfigurations by scanning assemblies.");
            // Scan all assemblies for implementations of the interface and create instances.
            return from assembly in BuildManager.GetReferencedAssemblies().Cast<Assembly>()
                   from type in GetConfigurationTypes(assembly)
                   select CreateConfigurationInstance(type);
        }

        static IEnumerable<Type> GetConfigurationTypes(Assembly assembly)
        {
            IEnumerable<Type> types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                // Some types failed to load, often due to a referenced assembly being missing.
                // This is not usually a problem, so just continue with whatever did load.
                types = exception.Types.Where(type => type != null);
            }
            return types.Where(IsCassetteConfigurationType);
        } 

        static bool IsCassetteConfigurationType(Type type)
        {
            return type.IsPublic
                   && type.IsClass
                   && !type.IsAbstract
                   && typeof(ICassetteConfiguration).IsAssignableFrom(type);
        }

        static ICassetteConfiguration CreateConfigurationInstance(Type type)
        {
            Trace.Source.TraceInformation("Creating {0}.", type.FullName);

            return (ICassetteConfiguration)Activator.CreateInstance(type);
        }

        static CassetteApplication CreateCassetteApplication()
        {
            lock (CreationLock)
            {
                var allConfigurations = GetAllConfigurations(GetCassetteConfigurationSection());
                var cacheVersion = GetConfigurationVersion(allConfigurations, HttpRuntime.AppDomainAppVirtualPath);
                var settings = new CassetteSettings(cacheVersion);
                var bundles = new BundleCollection(settings);

                foreach (var configuration in allConfigurations)
                {
                    Trace.Source.TraceInformation("Executing configuration {0}",
                                                  configuration.GetType().AssemblyQualifiedName);
                    configuration.Configure(bundles, settings);
                }

                var routing = new CassetteRouting(settings.UrlModifier, () => ((CassetteApplication)CassetteApplicationContainer.Instance.Application).BundleContainer);
                settings.UrlGenerator = routing;

                Trace.Source.TraceInformation("Creating Cassette application object");
                Trace.Source.TraceInformation("IsDebuggingEnabled: {0}", settings.IsDebuggingEnabled);
                Trace.Source.TraceInformation("Cache version: {0}", cacheVersion);

                var application = new CassetteApplication(
                    bundles,
                    settings,
                    routing,
                    GetCurrentHttpContext
                );

                application.InstallRoutes(RouteTable.Routes);

                return application;
            }
        }

        static List<ICassetteConfiguration> GetAllConfigurations(CassetteConfigurationSection section)
        {
            var initialConfiguration = new InitialConfiguration(
                section,
                GetSystemWebCompilationDebug(),
                HttpRuntime.AppDomainAppPath,
                HttpRuntime.AppDomainAppVirtualPath,
                _storage
            );

            var allConfigurations = new List<ICassetteConfiguration> { initialConfiguration };
            allConfigurations.AddRange(_configurations);
            return allConfigurations;
        }

        static CassetteConfigurationSection GetCassetteConfigurationSection()
        {
            return (WebConfigurationManager.GetSection("cassette") as CassetteConfigurationSection) 
                   ?? new CassetteConfigurationSection();
        }

        static bool GetSystemWebCompilationDebug()
        {
            var compilation = WebConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
            return compilation != null && compilation.Debug;
        }

        static string GetConfigurationVersion(IEnumerable<ICassetteConfiguration> configurations, string virtualDirectory)
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