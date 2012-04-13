using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Manifests;

namespace Cassette.MSBuild
{
    public class MSBuildHost : HostBase
    {
        readonly string inputDirectory;
        readonly string outputFilename;

        public MSBuildHost(string inputDirectory, string outputFilename)
        {
            this.inputDirectory = inputDirectory;
            this.outputFilename = outputFilename;
        }

        protected override IEnumerable<Assembly> LoadAssemblies()
        {
            return Directory
                .GetFiles(inputDirectory, "*.dll")
                .Select(Assembly.LoadFrom);
        }

        protected override bool CanCreateRequestLifetimeProvider
        {
            get { return false; }
        }

        protected override Type BundleCollectionInitializerType
        {
            get { return typeof(BundleCollectionInitializer); }
        }

        public void Execute()
        {
            var bundles = Container.Resolve<BundleCollection>();
            var settings = Container.Resolve<CassetteSettings>();
            WriteManifest(bundles, settings);
        }

        void WriteManifest(BundleCollection bundles, CassetteSettings settings)
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

        protected override CassetteSettings CreateSettings()
        {
            var settings = base.CreateSettings();
            settings.SourceDirectory = new FileSystemDirectory(Environment.CurrentDirectory);
            return settings;
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            // Override any URL modifier, even if set by the application.
            // So this is *after* the base.RegisterContainerItems() call.
            // We must output specially wrapped URLs at compile-time. These are then modified by the application at run-time.
            Container.Register<IUrlModifier>(new UrlPlaceholderWrapper());
        }
    }
}