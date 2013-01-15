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

        [ConfigurationProperty("cacheDirectory", DefaultValue = null)]
        public string CacheDirectory
        {
            get { return (string)this["cacheDirectory"]; }
            set { this["cacheDirectory"] = value; }
        }

        [ConfigurationProperty("compressionEnabled", DefaultValue = false)]
        public bool CompressionEnabled
        {
            get { return (bool)this["compressionEnabled"]; }
            set { this["compressionEnabled"] = value; }
        }
    }
}