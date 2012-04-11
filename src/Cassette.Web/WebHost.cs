using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Routing;
using Cassette.Configuration;
using Cassette.IO;
using TinyIoC;

namespace Cassette.Web
{
    public class WebHost : HostBase
    {
        protected override IEnumerable<Assembly> LoadAssemblies()
        {
            return BuildManager.GetReferencedAssemblies().Cast<Assembly>();
        }

        protected override TinyIoCContainer.ITinyIoCObjectLifetimeProvider RequestLifetimeProvider
        {
            get { return new HttpContextLifetimeProvider(() => Container.Resolve<HttpContextBase>()); }
        }

        protected override void RegisterContainerItems()
        {
            Container.Register(typeof(HttpContextBase), (c, p) => HttpContext());
            Container.Register(typeof(RouteCollection), Routes);
            Container.Register(typeof(IUrlModifier), new VirtualDirectoryPrepender(AppDomainAppVirtualPath));

            base.RegisterContainerItems();
        }

        protected virtual HttpContextBase HttpContext()
        {
            return new HttpContextWrapper(System.Web.HttpContext.Current);
        }

        protected virtual RouteCollection Routes
        {
            get { return RouteTable.Routes; }
        }

        protected override string GetHostVersion()
        {
            // Include the virtual directory so that if the application is moved to 
            // another virtual directory the bundles will be rebuilt with the updated URLs.
            return base.GetHostVersion() + AppDomainAppVirtualPath;
        }

        protected virtual string AppDomainAppVirtualPath
        {
            get { return HttpRuntime.AppDomainAppVirtualPath; }
        }

        protected override IEnumerable<Type> GetStartUpTaskTypes()
        {
            var startUpTaskTypes = base.GetStartUpTaskTypes();
            return startUpTaskTypes.Concat(new[]
            {
                typeof(RouteInstaller),
                typeof(FileSystemWatchingBundleRebuilder)
            });
        }

        protected override CassetteSettings Settings
        {
            get
            {
                var settings = base.Settings;
                var configurationSection = GetConfigurationSection();
                settings.IsDebuggingEnabled = configurationSection.Debug.HasValue ? configurationSection.Debug.Value : IsAspNetDebuggingEnabled;
                settings.IsHtmlRewritingEnabled = configurationSection.RewriteHtml;
                settings.AllowRemoteDiagnostics = configurationSection.AllowRemoteDiagnostics;
                settings.SourceDirectory = new FileSystemDirectory(AppDomainAppPath);
                settings.CacheDirectory = new IsolatedStorageDirectory(() => IsolatedStorageContainer.IsolatedStorageFile);
                settings.PrecompiledManifestFile = settings.SourceDirectory.GetFile(string.IsNullOrEmpty(configurationSection.PrecompiledManifest) ? "~/App_Data/cassette.xml" : configurationSection.PrecompiledManifest);
                return settings;
            }
        }

        protected virtual string AppDomainAppPath
        {
            get { return HttpRuntime.AppDomainAppPath; }
        }

        protected virtual CassetteConfigurationSection GetConfigurationSection()
        {
            return WebConfigurationManager.GetSection("cassette") as CassetteConfigurationSection;
        }

        protected virtual bool IsAspNetDebuggingEnabled
        {
            get
            {
                var compilation = WebConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
                return compilation != null && compilation.Debug;
            }
        }

        public void Hook(HttpApplication httpApplication)
        {
            var installer = Container.Resolve<PlaceholderReplacingResponseFilterInstaller>();
            installer.Install(httpApplication);
        }
    }
}