using System.Web;
using System.Web.Configuration;
using Cassette.IO;
using System.IO;

namespace Cassette.Aspnet
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
            settings.IsFileSystemWatchingEnabled = TrustLevel.IsFullTrust();
            settings.AllowRemoteDiagnostics = configurationSection.AllowRemoteDiagnostics;
            settings.SourceDirectory = new FileSystemDirectory(AppDomainAppPath);
            settings.CacheDirectory = GetCacheDirectory(configurationSection);

            // Include the virtual directory so that if the application is moved to 
            // another virtual directory the bundles will be rebuilt with the updated URLs.
            settings.Version += virtualDirectory;
        }

        IDirectory GetCacheDirectory(CassetteConfigurationSection configurationSection)
        {
            var path = configurationSection.CacheDirectory;
            if (string.IsNullOrEmpty(path))
            {
                return new IsolatedStorageDirectory(() => IsolatedStorageContainer.IsolatedStorageFile);
            }
            else if (Path.IsPathRooted(path))
            {
                return new FileSystemDirectory(path);
            }
            else
            {
                path = path.TrimStart('~', '/');
                return new FileSystemDirectory(Path.Combine(AppDomainAppPath, path));
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