using System.Configuration;

namespace Cassette.Configuration
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

        [ConfigurationProperty("precompiledManifest", DefaultValue = null)]
        public string PrecompiledManifest
        {
            get { return (string)this["precompiledManifest"]; }
            set { this["precompiledManifest"] = value; }
        }
    }
}