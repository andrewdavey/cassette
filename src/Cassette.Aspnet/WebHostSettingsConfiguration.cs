using System;
using System.Web;
using System.Web.Configuration;
using System.Xml.Linq;
using Cassette.IO;
using System.IO;
using System.Xml;
#if NET35
using Cassette.Utilities;
#endif

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
            settings.AllowRemoteDiagnostics = configurationSection.AllowRemoteDiagnostics;
            settings.SourceDirectory = new FileSystemDirectory(AppDomainAppPath);
            settings.CacheDirectory = GetCacheDirectory(configurationSection);
            settings.IsFileSystemWatchingEnabled = TrustLevel.IsFullTrust() && !IsStaticCacheManifest(settings);

            IsStaticCacheManifest(settings);

            // Include the virtual directory so that if the application is moved to 
            // another virtual directory the bundles will be rebuilt with the updated URLs.
            settings.Version += virtualDirectory;
        }

        IDirectory GetCacheDirectory(CassetteConfigurationSection configurationSection)
        {
            var path = configurationSection.CacheDirectory;
            if (string.IsNullOrEmpty(path))
            {
                var container = new IsolatedStorageContainer(configurationSection.IsolatedStoragePerDomain);
                return new IsolatedStorageDirectory(() => container.IsolatedStorageFile);
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

        bool IsStaticCacheManifest(CassetteSettings settings)
        {
            var manifestFile = settings.CacheDirectory.GetFile("manifest.xml");
            if (!manifestFile.Exists) return false;
            using (var stream = manifestFile.OpenRead())
            {
                var reader = XmlReader.Create(stream);
                var doc = XDocument.Load(reader);
                var attribute = doc.Root.Attribute("IsStatic");
                return attribute != null
                    && attribute.Value.Equals("true", StringComparison.OrdinalIgnoreCase);
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