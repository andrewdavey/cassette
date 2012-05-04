using System.Configuration;

namespace Cassette
{
    public sealed class CassetteConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("debug", DefaultValue = null)]
        public bool? Debug
        {
            get { return (bool?)this["debug"]; }
            set { this["debug"] = value; }
        }

        [ConfigurationProperty("rewriteHtml", DefaultValue = true)]
        public bool RewriteHtml
        {
            get { return (bool)this["rewriteHtml"]; }
            set { this["rewriteHtml"] = value; }
        }

        [ConfigurationProperty("allowRemoteDiagnostics", DefaultValue = false)]
        public bool AllowRemoteDiagnostics
        {
            get { return (bool)this["allowRemoteDiagnostics"]; }
            set { this["allowRemoteDiagnostics"] = value; }
        }

        [ConfigurationProperty("watchFileSystem", DefaultValue = null)]
        public bool? WatchFileSystem
        {
            get { return (bool?)this["watchFileSystem"]; }
            set { this["watchFileSystem"] = value; }
        }

        // TODO: Remove this in favour of simply "cacheDirectory"
        // But add way to mark the cache as "compile-time" so we can skip checking it against the source directory.
        [ConfigurationProperty("precompiledCacheDirectory", DefaultValue = null)]
        public string PrecompiledManifest
        {
            get { return (string)this["precompiledCacheDirectory"]; }
            set { this["precompiledCacheDirectory"] = value; }
        }
    }
}