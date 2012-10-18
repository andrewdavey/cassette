using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Utilities;

namespace Cassette.RequireJS
{
    public class RequireJsConfigUrlProvider
    {
        readonly IConfigurationScriptBuilder configurationScriptBuilder;
        readonly CassetteSettings cassetteSettings;
        readonly IUrlGenerator urlGenerator;
        readonly object urlLock = new object();

        public RequireJsConfigUrlProvider(BundleCollection bundles, IConfigurationScriptBuilder configurationScriptBuilder, CassetteSettings cassetteSettings, IUrlGenerator urlGenerator)
        {
            this.configurationScriptBuilder = configurationScriptBuilder;
            this.cassetteSettings = cassetteSettings;
            this.urlGenerator = urlGenerator;
            bundles.Changed += BundlesChanged;
        }

        public string Url { get; private set; }

        void BundlesChanged(object sender, BundleCollectionChangedEventArgs e)
        {
            // Multiple concurrent bundle change events could cause problems.
            // So lock to avoid race conditions
            lock (urlLock)
            {
                Url = BuildUrl(e.Bundles);
            }
        }

        string BuildUrl(IEnumerable<Bundle> bundles)
        {
            var scriptBundles = bundles.OfType<ScriptBundle>().ToArray();
            var config = configurationScriptBuilder.BuildConfigurationScript(scriptBundles);
            var filename = GetCacheFilename(scriptBundles);
            WriteConfigCacheFile(filename, config);
            return urlGenerator.CreateCachedFileUrl("~/Cassette.RequireJS/" + filename);
        }

        void WriteConfigCacheFile(string filename, string config)
        {
            var directory = GetEmptyConfigCacheDirectory();

            var file = directory.GetFile(filename);
            using (var stream = file.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(config);
            }
        }

        IDirectory GetEmptyConfigCacheDirectory()
        {
            var directory = cassetteSettings.CacheDirectory.GetDirectory("Cassette.RequireJS");
            if (directory.Exists) directory.Delete();
            directory.Create();
            return directory;
        }

        static string GetCacheFilename(IEnumerable<ScriptBundle> scriptBundles)
        {
            string filename;
            var allHashes = scriptBundles
                .Select(b => b.Hash)
                .Aggregate<IEnumerable<byte>>((a, b) => a.Concat(b))
                .ToArray();
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(allHashes);
                return hash.ToUrlSafeBase64String() + ".js";
            }
        }
    }
}