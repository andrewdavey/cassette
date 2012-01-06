using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cassette
{
    public class CreateBundles : AppDomainIsolatedTask
    {
        readonly IUrlGenerator urlGenerator;

        public CreateBundles()
        {
            urlGenerator = new LocalUrlGenerator();
        }

        /// <summary>
        /// The source directory of the web application.
        /// </summary>
        public string SourceDir { get; set; }

        /// <summary>
        /// The web application assembly filename.
        /// </summary>
        [Required]
        public string Assembly { get; set; }

        /// <summary>
        /// Bundle files are saved in this directory. This can be relative to <see cref="SourceDir"/> or an absolute path.
        /// </summary>
        [Required]
        public string OutputDir { get; set; }

        public override bool Execute()
        {
            AssignDefaults();
            if (!ValidateProperties()) return false;

            var configurations = CreateConfigurations();

            var settings = new CassetteSettings("")
            {
                SourceDirectory = new FileSystemDirectory(SourceDir),
                UrlGenerator = urlGenerator
            };

            var bundles = new BundleCollection(settings);
            foreach (var configuration in configurations)
            {
                configuration.Configure(bundles, settings);
            }

            OutputBundles(bundles, settings);

            return true;
        }

        void AssignDefaults()
        {
            if (string.IsNullOrEmpty(SourceDir))
            {
                SourceDir = Environment.CurrentDirectory;
            }

            if (!Path.IsPathRooted(OutputDir))
            {
                OutputDir = Path.Combine(SourceDir, OutputDir);
            }

            Assembly = Path.Combine(SourceDir, Assembly);
        }

        bool ValidateProperties()
        {
            if (!Directory.Exists(SourceDir))
            {
                Log.LogError("SourceDir not found: {0}", SourceDir);
                return false;
            }

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
            return from type in assembly.GetExportedTypes()
                   where type.IsClass && 
                         !type.IsAbstract && 
                         typeof(ICassetteConfiguration).IsAssignableFrom(type)
                   select (ICassetteConfiguration)Activator.CreateInstance(type);
        }

        void OutputBundles(BundleCollection bundles, CassetteSettings settings)
        {
            foreach (var bundle in bundles)
            {
                OutputBundle(bundle, settings);
            }
        }

        void OutputBundle(Bundle bundle, CassetteSettings settings)
        {
            Log.LogMessage("{0}: {1}", bundle.GetType().Name, bundle.Path);
            foreach (var asset in bundle.Assets)
            {
                Log.LogMessage("  " + asset.SourceFile.FullPath);
            }

            bundle.Process(settings);

            OutputBundleContent(bundle);
            OutputRawFiles(SourceDir, bundle);
        }

        void OutputBundleContent(Bundle bundle)
        {
            var url = urlGenerator.CreateBundleUrl(bundle);
            var outputFilename = Path.Combine(OutputDir, url);
            EnsureDirectoryExists(outputFilename);
            Log.LogMessage("Url: " + url);

            using (var bundleStream = bundle.OpenStream())
            using (var file = File.OpenWrite(outputFilename))
            {
                bundleStream.CopyTo(file);
                file.Flush();
            }
        }

        void OutputRawFiles(string root, Bundle bundle)
        {
            var collector = new CollectRawFileReferencePaths();
            bundle.Accept(collector);
            foreach (var path in collector.RawFilePaths)
            {
                OutputRawFile(root, path);
            }
        }

        void OutputRawFile(string root, string path)
        {
            var filename = Path.Combine(root, path.TrimStart('~', '/'));
            using (var fileStream = File.OpenRead(filename))
            {
                var hash = fileStream.ComputeSHA1Hash().ToHexString();
                var url = urlGenerator.CreateRawFileUrl(path, hash);
                var outputFilename = Path.Combine(OutputDir, url);
                EnsureDirectoryExists(outputFilename);
                File.Copy(filename, outputFilename, true);
            }
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
