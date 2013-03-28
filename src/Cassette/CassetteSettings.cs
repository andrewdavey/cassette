using System.Collections.Generic;
using System.Linq;
using Cassette.IO;

#if NET35
using Cassette.Utilities;
#endif

namespace Cassette
{
    /// <summary>
    /// Settings that control Cassette's behavior.
    /// </summary>
    public class CassetteSettings
    {
        public CassetteSettings(IEnumerable<IConfiguration<CassetteSettings>> configurations)
        {
            ApplyConfigurations(configurations);
        }

        internal CassetteSettings() // Tests don't usually need to specify configurations, so give them a default constructor to use.
            : this(Enumerable.Empty<IConfiguration<CassetteSettings>>())
        {
        }

        void ApplyConfigurations(IEnumerable<IConfiguration<CassetteSettings>> configurations)
        {
            configurations.OrderByConfigurationOrderAttribute().Configure(this);
        }

        /// <summary>
        /// When this property is true, Cassette will output debug-friendly assets. When false, combined, minified bundles are used instead.
        /// </summary>
        public bool IsDebuggingEnabled { get; set; }

        public bool IsFileNameWithHashDisabled { get; set; }

        /// <summary>
        /// When true (the default), Cassette will buffer page output and rewrite to allow bundle references to be inserted into &lt;head&gt;
        /// after it has already been rendered. Disable this when &lt;system.webServer&gt;/&lt;urlCompression dynamicCompressionBeforeCache="true"&gt;
        /// is in Web.config.
        /// </summary>
        public bool IsHtmlRewritingEnabled { get; set; }

        /// <summary>
        /// The directory containing the original bundle asset files.
        /// </summary>
        public IDirectory SourceDirectory { get; set; }

        /// <summary>
        /// The directory used to cache combined, minified bundles.
        /// </summary>
        public IDirectory CacheDirectory { get; set; }

        public bool AllowRemoteDiagnostics { get; set; }

        public string Version { get; set; }

        public bool IsFileSystemWatchingEnabled { get; set; }
    }
}