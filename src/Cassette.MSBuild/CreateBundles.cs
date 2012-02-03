using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.Configuration;
using Cassette.Manifests;
using Cassette.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cassette.MSBuild
{
    public class CreateBundles : AppDomainIsolatedTask
    {
        readonly IUrlGenerator urlGenerator;

        public CreateBundles()
        {
            urlGenerator = new LocalUrlGenerator();
        }

        /// <summary>
        /// The web application assembly filename.
        /// </summary>
        [Required]
        public string Assembly { get; set; }

        [Required]
        public string Output { get; set; }

        public override bool Execute()
        {
            if (!ValidateProperties()) return false;

            var configurations = CreateConfigurations();
            var settings = new CassetteSettings("")
            {
                UrlGenerator = urlGenerator
            };
            var bundles = new BundleCollection(settings);
            foreach (var configuration in configurations)
            {
                configuration.Configure(bundles, settings);
            }
            foreach (var bundle in bundles)
            {
                bundle.Process(settings);
            }

            var manifest = new CassetteManifest("", bundles.Select(bundle => bundle.CreateBundleManifest(true)));
            using (var file = File.OpenWrite(Output))
            {
                var writer = new CassetteManifestWriter(file);
                writer.Write(manifest);
            }

            return true;
        }

        bool ValidateProperties()
        {
            if (!File.Exists(Assembly))
            {
                Log.LogError("Assembly not found: {0}", Assembly);
                return false;
            }

            return true;
        }

        IEnumerable<ICassetteConfiguration> CreateConfigurations()
        {
            var assembly = System.Reflection.Assembly.LoadFrom(Assembly);
            var scanner = new AssemblyScanningCassetteConfigurationFactory(new[] { assembly });
            return scanner.CreateCassetteConfigurations();
        }

        void EnsureDirectoryExists(string outputFilename)
        {
            new FileInfo(outputFilename).Directory.Create();
        }
    }

    class LocalUrlGenerator : IUrlGenerator
    {
        public string CreateBundleUrl(Bundle bundle)
        {
            return (string.Format("{0}/{1}_{2}",
                ConventionalBundlePathName(bundle.GetType()),
                bundle.PathWithoutPrefix,
                bundle.Hash.ToHexString()
            ));
        }

        public string CreateAssetUrl(IAsset asset)
        {
            return (string.Format(
                "asset/{0}?{1}",
                asset.SourceFile.FullPath.Substring(2),
                asset.Hash.ToHexString()
            ));
        }

        public string CreateRawFileUrl(string filename, string hash)
        {
            if (filename.StartsWith("~") == false)
            {
                throw new ArgumentException("Image filename must be application relative (starting with '~').");
            }

            filename = filename.Substring(2); // Remove the "~/"
            var dotIndex = filename.LastIndexOf('.');
            var name = filename.Substring(0, dotIndex);
            var extension = filename.Substring(dotIndex + 1);

            return (string.Format("file/{0}_{1}_{2}",
                ConvertToForwardSlashes(name),
                hash,
                extension
            ));
        }

        static string ConventionalBundlePathName(Type bundleType)
        {
            // ExternalScriptBundle subclasses ScriptBundle, but we want the name to still be "scripts"
            // So walk up the inheritance chain until we get to something that directly inherits from Bundle.
            while (bundleType != null && bundleType.BaseType != typeof(Bundle))
            {
                bundleType = bundleType.BaseType;
            }
            if (bundleType == null) throw new ArgumentException("Type must be a subclass of Cassette.Bundle.", "bundleType");

            return bundleType.Name.ToLowerInvariant();
        }

        string ConvertToForwardSlashes(string path)
        {
            return path.Replace('\\', '/');
        }
    }

    class CollectRawFileReferencePaths : IBundleVisitor
    {
        readonly List<string> rawFilePaths = new List<string>();

        public IEnumerable<string> RawFilePaths
        {
            get { return rawFilePaths.Distinct(StringComparer.OrdinalIgnoreCase); }
        }

        public void Visit(Bundle bundle)
        {   
        }

        public void Visit(IAsset asset)
        {
            var paths = asset.References
                .Where(r => r.Type == AssetReferenceType.RawFilename)
                .Select(r => r.Path);
            rawFilePaths.AddRange(paths);
        }
    }
}
