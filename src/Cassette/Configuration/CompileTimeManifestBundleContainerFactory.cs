using System.IO;
using System.Linq;
using Cassette.Manifests;

namespace Cassette.Configuration
{
    class CompileTimeManifestBundleContainerFactory : BundleContainerFactoryBase
    {
        readonly string filename;
        readonly IUrlModifier urlModifier;

        public CompileTimeManifestBundleContainerFactory(string filename, CassetteSettings settings, IBundleFactoryProvider bundleFactoryProvider, IUrlModifier urlModifier)
            : base(settings, bundleFactoryProvider)
        {
            this.filename = filename;
            this.urlModifier = urlModifier;
        }

        public override IBundleContainer CreateBundleContainer()
        {
            Trace.Source.TraceInformation("Initializing bundles from compile-time manifest: {0}", filename);

            var manifest = ReadCassetteManifest();
            var bundles = manifest.CreateBundles(urlModifier);

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