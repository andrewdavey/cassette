using System.IO;
using System.Linq;
using Cassette.Configuration;
using Cassette.Manifests;

namespace Cassette.MSBuild
{
    class CreateBundlesImplementation
    {
        readonly string outputFilename;
        readonly BundleCollection bundles;
        readonly CassetteSettings settings;

        public CreateBundlesImplementation(string outputFilename, BundleCollection bundles, CassetteSettings settings)
        {
            this.outputFilename = outputFilename;
            this.bundles = bundles;
            this.settings = settings;
        }

        public void Execute()
        {
            WriteManifest();
        }

        void WriteManifest()
        {
            var file = settings.SourceDirectory.GetFile(outputFilename);
            using (var outputStream = file.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var writer = new CassetteManifestWriter(outputStream);
                var manifest = new CassetteManifest("", bundles.Select(bundle => bundle.CreateBundleManifest(true)));
                writer.Write(manifest);
                outputStream.Flush();
            }
        }
    }
}