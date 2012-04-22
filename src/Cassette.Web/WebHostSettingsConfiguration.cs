using System.Web;
using System.Web.Configuration;
using Cassette.Configuration;
using Cassette.IO;

namespace Cassette.Web
{
    class WebHostSettingsConfiguration : IConfiguration<CassetteSettings>
    {
        readonly string virtualDirectory;

        public WebHostSettingsConfiguration(string virtualDirectory)
        {
            this.virtualDirectory = virtualDirectory;
        }

        public void Configure(CassetteSettings settings)
        {
            var configurationSection = GetConfigurationSection();
            settings.IsDebuggingEnabled = configurationSection.Debug.HasValue ? configurationSection.Debug.Value : IsAspNetDebuggingEnabled;
            settings.IsHtmlRewritingEnabled = configurationSection.RewriteHtml;
            settings.AllowRemoteDiagnostics = configurationSection.AllowRemoteDiagnostics;
            settings.SourceDirectory = new FileSystemDirectory(AppDomainAppPath);
            settings.CacheDirectory = new IsolatedStorageDirectory(() => IsolatedStorageContainer.IsolatedStorageFile);
            var precompiledManifestFilename = string.IsNullOrEmpty(configurationSection.PrecompiledManifest)
                                                  ? "~/App_Data/cassette.xml"
                                                  : configurationSection.PrecompiledManifest;
            settings.PrecompiledManifestFile = settings.SourceDirectory.GetFile(precompiledManifestFilename);

            // Include the virtual directory so that if the application is moved to 
            // another virtual directory the bundles will be rebuilt with the updated URLs.
            settings.Version += virtualDirectory;

            if (settings.PrecompiledManifestFile.Exists)
            {
                settings.IsDebuggingEnabled = false;
            }
        }

        protected virtual string AppDomainAppPath
        {
            get { return HttpRuntime.AppDomainAppPath; }
        }

        protected virtual CassetteConfigurationSection GetConfigurationSection()
        {
            return (WebConfigurationManager.GetSection("cassette") as CassetteConfigurationSection)
                   ?? new CassetteConfigurationSection();
        }

        protected virtual bool IsAspNetDebuggingEnabled
        {
            get
            {
                var compilation = WebConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
                return compilation != null && compilation.Debug;
            }
        }
    }
}