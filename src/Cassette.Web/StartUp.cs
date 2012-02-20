﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        static CassetteApplicationContainer<CassetteApplication> _container;
        static readonly StartUpTraceRecorder StartUpTraceRecorder = new StartUpTraceRecorder();

        // ReSharper disable UnusedMember.Global
        public static void PreApplicationStart()
        {
            StartUpTraceRecorder.Start();
            Trace.Source.TraceInformation("Registering CassetteHttpModule.");
            DynamicModuleUtility.RegisterModule(typeof(CassetteHttpModule));
        }
        // ReSharper restore UnusedMember.Global

        // ReSharper disable UnusedMember.Global
        // This runs *after* Global.asax Application_Start.
        public static void PostApplicationStart()
        {
            Trace.Source.TraceInformation("PostApplicationStart.");
            try
            {
                InitializeApplicationContainer();
                InstallRoutes();
                Trace.Source.TraceInformation("Cassette startup completed. It took {0} ms.", StartUpTraceRecorder.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                Trace.Source.TraceEvent(TraceEventType.Error, 0, ex.Message);
                throw;
            }
            finally
            {
                StartUpTraceRecorder.Stop();
            }
        }
        // ReSharper restore UnusedMember.Global

        static void InitializeApplicationContainer()
        {
            var factory = CreateApplicationContainerFactory();
            _container = factory.CreateContainer();
            CassetteApplicationContainer.SetApplicationAccessor(() => _container.Application);
            var forceCreation = _container.Application;
        }

        static CassetteApplicationContainerFactory CreateApplicationContainerFactory()
        {
            return new CassetteApplicationContainerFactory(
                CreateCassetteConfigurationFactory(),
                GetCassetteConfigurationSection,
                HttpRuntime.AppDomainAppPath,
                HttpRuntime.AppDomainAppVirtualPath,
                IsAspNetDebugging,
                GetCurrentHttpContext
            );
        }

        static HttpContextBase GetCurrentHttpContext()
        {
            return new HttpContextWrapper(HttpContext.Current);
        }

        static void InstallRoutes()
        {
            var routing = new RouteInstaller(_container, UrlGenerator.RoutePrefix);
            routing.InstallRoutes(RouteTable.Routes);
        }

        // ReSharper disable UnusedMember.Global
        public static void ApplicationShutdown()
        {
            Trace.Source.TraceInformation("Application shutdown - disposing resources.");
            _container.Dispose();
            IsolatedStorageContainer.Dispose();
            Scripts.IECoffeeScriptCompiler.SingleThreadedWorker.Singleton.Stop();
        }
        // ReSharper restore UnusedMember.Global

        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        /// <summary>
        /// Set this optional property to provide a custom function that creates the application's Cassette configuration objects.
        /// Assignment to this property should happen in Application_Start.
        /// If not set, Cassette's default assembly scanner will look for configuration types to create.
        /// </summary>
        public static Func<IEnumerable<ICassetteConfiguration>> CreateConfigurations { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
        // ReSharper restore MemberCanBePrivate.Global

        internal static string TraceOutput
        {
            get { return StartUpTraceRecorder.TraceOutput; }
        }

        static CassetteConfigurationSection GetCassetteConfigurationSection
        {
            get
            {
                return (WebConfigurationManager.GetSection("cassette") as CassetteConfigurationSection)
                       ?? new CassetteConfigurationSection();
            }
        }

        static bool IsAspNetDebugging
        {
            get
            {
                var compilation = WebConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
                return compilation != null && compilation.Debug;
            }
        }

        static ICassetteConfigurationFactory CreateCassetteConfigurationFactory()
        {
            if (CreateConfigurations == null)
            {
                return new AssemblyScanningCassetteConfigurationFactory(GetApplicationAssemblies());
            }
            else
            {
                return new DelegateCassetteConfigurationFactory(CreateConfigurations);
            }
        }

        static IEnumerable<Assembly> GetApplicationAssemblies()
        {
            return BuildManager.GetReferencedAssemblies().Cast<Assembly>();
        }
    }
}