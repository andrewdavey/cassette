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
    public class DefaultBootstrapper : DefaultBootstrapperBase
    {
        protected override IEnumerable<InstanceRegistration> InstanceRegistrationTypes
        {
            get
            {
                return base.InstanceRegistrationTypes.Concat(new[]
                {
                    new InstanceRegistration(typeof(Func<HttpContextBase>), new Func<HttpContextBase>(() => new HttpContextWrapper(HttpContext.Current))),
                    new InstanceRegistration(typeof(RouteCollection), RouteTable.Routes)
                });
            }
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
    }
}