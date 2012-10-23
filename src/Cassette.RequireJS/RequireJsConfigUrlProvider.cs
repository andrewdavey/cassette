using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.RequireJS
{
    class RequireJsConfigUrlProvider : IRequireJsConfigUrlProvider
    {
        readonly IAmdConfiguration modules;
        readonly IConfigurationScriptBuilder configurationScriptBuilder;
        readonly IDirectory cacheDirectory;
        readonly IUrlGenerator urlGenerator;
        readonly object urlLock = new object();

        public RequireJsConfigUrlProvider(
            BundleCollection bundles,
            IAmdConfiguration modules,
            IConfigurationScriptBuilder configurationScriptBuilder,
            IDirectory cacheDirectory,
            IUrlGenerator urlGenerator)
        {
            this.modules = modules;
            this.configurationScriptBuilder = configurationScriptBuilder;
            this.cacheDirectory = cacheDirectory;
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
                Url = BuildUrl();
            }
        }

        string BuildUrl()
        {
            var config = configurationScriptBuilder.BuildConfigurationScript(modules);
            var filename = GetCacheFilename();
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
            var directory = cacheDirectory.GetDirectory("Cassette.RequireJS");
            if (directory.Exists) directory.Delete();
            directory.Create();
            return directory;
        }

        string GetCacheFilename()
        {
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(ConcatAllBundleHashes());
                return hash.ToHexString() + ".js";
            }
        }

        byte[] ConcatAllBundleHashes()
        {
            return modules
                .Select(m => m.Bundle.Hash)
                .Aggregate<IEnumerable<byte>>(Enumerable.Concat)
                .ToArray();
        }
    }
}