using System.Collections.Generic;
using System.Linq;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Manifests;

namespace Cassette.MSBuild
{
    class CreateBundlesImplementation
    {
        readonly ICassetteManifestWriter manifestWriter;
        readonly CassetteSettings settings;
        readonly BundleCollection bundles;
        readonly IEnumerable<ICassetteConfiguration> configurations;

        public CreateBundlesImplementation(ICassetteConfigurationFactory configurationFactory, ICassetteManifestWriter manifestWriter, IDirectory sourceDirectory)
        {
            this.manifestWriter = manifestWriter;
            configurations = configurationFactory.CreateCassetteConfigurations();
            settings = new CassetteSettings("")
            {
                SourceDirectory = sourceDirectory
            };
            bundles = new BundleCollection(settings);
        }

        public void Execute()
        {
            Configure();
            AssignUrlModifier();
            AssignUrlGeneratorIfNull();
            ProcessBundles();
            WriteManifest();
        }

        void Configure()
        {
            foreach (var configuration in configurations)
            {
                configuration.Configure(bundles, settings);
            }
        }

        void AssignUrlModifier()
        {
            // The URLs saved in the manifest data MUST be wrapped using a special placeholder.
            // This is so Cassette can parse them at runtime and call the application's own IUrlModifier.
            settings.UrlModifier = new UrlPlaceholderWrapper();
        }

        void AssignUrlGeneratorIfNull()
        {
            if (settings.UrlGenerator == null)
            {
                settings.UrlGenerator = new UrlGenerator(settings.UrlModifier, UrlGenerator.RoutePrefix);
            }
        }

        void ProcessBundles()
        {
            foreach (var bundle in bundles)
            {
                bundle.Process(settings);
            }
        }

        void WriteManifest()
        {
            var manifest = new CassetteManifest("", bundles.Select(bundle => bundle.CreateBundleManifest(true)));
            manifestWriter.Write(manifest);
        }
    }
}