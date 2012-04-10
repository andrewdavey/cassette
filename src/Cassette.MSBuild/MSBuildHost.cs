using System;
using System.IO;
using System.Linq;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Manifests;

namespace Cassette.MSBuild
{
    public class MSBuildHost : HostBase
    {
        public string OutputFilename { get; set; }

        public void Execute()
        {
            var bundles = Container.Resolve<BundleCollection>();
            var settings = Container.Resolve<CassetteSettings>();
            WriteManifest(bundles, settings);
        }

        void WriteManifest(BundleCollection bundles, CassetteSettings settings)
        {
            var file = settings.SourceDirectory.GetFile(OutputFilename);
            using (var outputStream = file.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var writer = new CassetteManifestWriter(outputStream);
                var manifest = new CassetteManifest("", bundles.Select(bundle => bundle.CreateBundleManifest(true)));
                writer.Write(manifest);
                outputStream.Flush();
            }
        }

        protected override CassetteSettings Settings
        {
            get
            {
                var settings = base.Settings;
                settings.SourceDirectory = new FileSystemDirectory(Environment.CurrentDirectory);
                return settings;
            }
        }

        protected override void RegisterContainerItems()
        {
            base.RegisterContainerItems();

            // Override any URL modifier, even if set by the application.
            // So this is *after* the base.RegisterContainerItems() call.
            // We must output specially wrapped URLs at compile-time. These are then modified by the application at run-time.
            Container.Register<IUrlModifier>(new UrlPlaceholderWrapper());
        }
    }
}