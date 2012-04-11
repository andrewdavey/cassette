using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.Views
{
    /// <summary>
    /// Provides the implementation for the static <see cref="Bundles"/> class.
    /// </summary>
    public class BundlesHelper : IStartUpTask
    {
        readonly BundleCollection bundles;
        readonly CassetteSettings settings;
        readonly IUrlGenerator urlGenerator;
        readonly Func<IReferenceBuilder> getReferenceBuilder;

        public BundlesHelper(BundleCollection bundles, CassetteSettings settings, IUrlGenerator urlGenerator, Func<IReferenceBuilder> getReferenceBuilder)
        {
            this.bundles = bundles;
            this.settings = settings;
            this.urlGenerator = urlGenerator;
            this.getReferenceBuilder = getReferenceBuilder;
        }

        void IStartUpTask.Run()
        {
            // At application start-up, Cassette's infrastructure will create an instance of this class
            // because it implements IStartUpTask.
            // Cassette then calls Run(). So we set the static Bundles class Helper implementation here.
            Bundles.Helper = this;
        }

        public void Reference(string assetPathOrBundlePathOrUrl, string pageLocation)
        {
            getReferenceBuilder().Reference(assetPathOrBundlePathOrUrl, pageLocation);
        }

        public void Reference(Bundle bundle, string pageLocation)
        {
            getReferenceBuilder().Reference(bundle, pageLocation);
        }
        
        public IHtmlString Render<T>(string location) where T : Bundle
        {
            return new HtmlString(getReferenceBuilder().Render<T>(location));
        }

        public IEnumerable<Bundle> GetReferencedBundles(string pageLocation)
        {
            return getReferenceBuilder().GetBundles(pageLocation);
        }

        public IEnumerable<string> GetReferencedBundleUrls<T>(string pageLocation)
            where T : Bundle
        {
            var referencedBundles = GetReferencedBundles(pageLocation).OfType<T>();

            if (settings.IsDebuggingEnabled)
            {
                return referencedBundles
                    .SelectMany(GetAllAssets)
                    .Select(urlGenerator.CreateAssetUrl);
            }
            else
            {
                return referencedBundles
                    .Select(urlGenerator.CreateBundleUrl);
            }
        }

        public string Url<T>(string bundlePath)
            where T : Bundle
        {
            var bundle = bundles.FindBundlesContainingPath(bundlePath).OfType<T>().FirstOrDefault();
            if (bundle == null)
            {
                throw new ArgumentException(string.Format("Bundle not found with path \"{0}\".", bundlePath));
            }

            return urlGenerator.CreateBundleUrl(bundle);
        }

        static IEnumerable<IAsset> GetAllAssets(Bundle bundle)
        {
            var collector = new AssetCollector();
            bundle.Accept(collector);
            return collector.Assets;
        }

        class AssetCollector : IBundleVisitor
        {
            public AssetCollector()
            {
                Assets = new List<IAsset>();
            }

            public List<IAsset> Assets { get; private set; }

            public void Visit(Bundle bundle)
            {
            }

            public void Visit(IAsset asset)
            {
                Assets.Add(asset);
            }
        }

        public string FileUrl(string applicationRelativeFilePath)
        {
            applicationRelativeFilePath = PathUtilities.AppRelative(applicationRelativeFilePath);

            var file = settings.SourceDirectory.GetFile(applicationRelativeFilePath);
            ThrowIfFileNotFound(applicationRelativeFilePath, file);
            ThrowIfCannotRequestRawFile(applicationRelativeFilePath, file, settings);

            using (var stream = file.OpenRead())
            {
                var hash = stream.ComputeSHA1Hash().ToHexString();
                return urlGenerator.CreateRawFileUrl(applicationRelativeFilePath, hash);
            }
        }

        static void ThrowIfCannotRequestRawFile(string applicationRelativeFilePath, IFile file, CassetteSettings settings)
        {
            if (settings.CanRequestRawFile(file.FullPath)) return;

            throw new Exception(
                string.Format(
                    "The file {0} cannot be requested. In CassetteConfiguration, use the settings.AllowRawFileAccess method to tell Cassette which files are safe to request.",
                    applicationRelativeFilePath
                )
            );
        }

        static void ThrowIfFileNotFound(string applicationRelativeFilePath, IFile file)
        {
            if (file.Exists) return;
            throw new FileNotFoundException(
                "Cannot find file " + applicationRelativeFilePath,
                applicationRelativeFilePath
            );
        }
    }
}