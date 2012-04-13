using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.IO;
#if NET35
using Cassette.Utilities;
#endif

namespace Cassette.Configuration
{
    /// <summary>
    /// Settings that control Cassette's behavior.
    /// </summary>
    public class CassetteSettings
    {
        readonly List<Func<string, bool>> allowPathPredicates = new List<Func<string, bool>>();
 
        internal IFile PrecompiledManifestFile { get; set; }

        /// <summary>
        /// When this property is true, Cassette will output debug-friendly assets. When false, combined, minified bundles are used instead.
        /// </summary>
        public bool IsDebuggingEnabled { get; set; }

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

        internal bool AllowRemoteDiagnostics { get; set; }

        internal string Version { get; set; }

        internal bool CanRequestRawFile(string filePath)
        {
            return allowPathPredicates.Any(predicate => predicate(filePath));
        }

        public void AllowRawFileRequest(Func<string, bool> pathIsAllowed)
        {
            allowPathPredicates.Add(pathIsAllowed);
        }
    }
}