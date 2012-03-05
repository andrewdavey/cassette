using System.IO;
using System.Linq;
using Cassette.Manifests;

namespace Cassette.Configuration
{
    class CompileTimeManifestBundleContainerFactory : BundleContainerFactoryBase
    {
        readonly string filename;
        readonly CassetteSettings settings;

        public CompileTimeManifestBundleContainerFactory(string filename, CassetteSettings settings) : base(settings)
        {
            this.filename = filename;
            this.settings = settings;
        }

        public override IBundleContainer CreateBundleContainer()
        {
            Trace.Source.TraceInformation("Initializing bundles from compile-time manifest: {0}", filename);

            var manifest = ReadCassetteManifest();
            var bundles = manifest.CreateBundleCollection(settings);

            var externalBundles = CreateExternalBundlesUrlReferences(bundles);
            return new BundleContainer(bundles.Concat(externalBundles));
        }

        CassetteManifest ReadCassetteManifest()
        {
            using (var file = OpenManifestFile())
            {
                var reader = new CassetteManifestReader(file);
                var manifest = reader.Read();
                return manifest;
            }
        }

        FileStream OpenManifestFile()
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("Cannot find the file \"{0}\" specified by precompiledManifest in the <cassette> configuration section.", filename);
            }
            return File.OpenRead(filename);
        }
    }
}