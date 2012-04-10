using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Routing;
using Cassette.Configuration;
using Cassette.IO;

namespace Cassette.Web
{
    public class WebHost : HostBase
    {
        protected override void RegisterContainerItems()
        {
            Container.Register(typeof(HttpContextBase), (c, p) => new HttpContextWrapper(HttpContext.Current));
            Container.Register(typeof(RouteCollection), RouteTable.Routes);
            Container.Register(typeof(IUrlModifier), new VirtualDirectoryPrepender(HttpRuntime.AppDomainAppVirtualPath));

            base.RegisterContainerItems();
        }

        protected override IEnumerable<Type> GetStartUpTaskTypes()
        {
            var startUpTaskTypes = base.GetStartUpTaskTypes();
            return startUpTaskTypes.Concat(new[]
            {
                typeof(RouteInstaller),
                typeof(FileSystemWatchingBundleRebuilder),
                typeof(UrlHelperExtensions)
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
                settings.SourceDirectory = new FileSystemDirectory(HttpRuntime.AppDomainAppPath);
                settings.CacheDirectory = new IsolatedStorageDirectory(() => IsolatedStorageContainer.IsolatedStorageFile);
                settings.PrecompiledManifestFile = settings.SourceDirectory.GetFile(string.IsNullOrEmpty(configurationSection.PrecompiledManifest) ? "~/App_Data/cassette.xml" : configurationSection.PrecompiledManifest);
                return settings;
            }
        }

        static CassetteConfigurationSection GetConfigurationSection()
        {
            return WebConfigurationManager.GetSection("cassette") as CassetteConfigurationSection;
        }

        static bool IsAspNetDebuggingEnabled
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