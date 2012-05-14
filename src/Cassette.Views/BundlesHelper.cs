using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Cassette.IO;
using Cassette.Utilities;
#if !NET35

#endif

namespace Cassette.Views
{
    /// <summary>
    /// Provides the implementation for the static <see cref="Bundles"/> class.
    /// </summary>
    public class BundlesHelper : IStartUpTask, IBundlesHelper
    {
        readonly BundleCollection bundles;
        readonly CassetteSettings settings;
        readonly IUrlGenerator urlGenerator;
        readonly Func<IReferenceBuilder> getReferenceBuilder;
        readonly IFileAccessAuthorization fileAccessAuthorization;

        public BundlesHelper(BundleCollection bundles, CassetteSettings settings, IUrlGenerator urlGenerator, Func<IReferenceBuilder> getReferenceBuilder, IFileAccessAuthorization fileAccessAuthorization)
        {
            this.bundles = bundles;
            this.settings = settings;
            this.urlGenerator = urlGenerator;
            this.getReferenceBuilder = getReferenceBuilder;
            this.fileAccessAuthorization = fileAccessAuthorization;
        }

        void IStartUpTask.Start()
        {
            Trace.Source.TraceInformation("Initializing Bundles helper");
            // At application start-up, Cassette's infrastructure will create an instance of this class because it implements IStartUpTask.
            // Cassette then calls Start(). So we set the static Bundles class Helper implementation here.
            Bundles.Helper = this;
        }

        public void Reference(string assetPathOrBundlePathOrUrl, string pageLocation)
        {
            getReferenceBuilder().Reference(assetPathOrBundlePathOrUrl, pageLocation);
        }

        public void Reference<T>(string assetPathOrBundlePathOrUrl, string pageLocation)
            where T: Bundle
        {
            getReferenceBuilder().Reference<T>(assetPathOrBundlePathOrUrl, pageLocation);            
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
                    .SelectMany(AssetCollector.GetAllAssets)
                    .Select(urlGenerator.CreateAssetUrl);
            }
            else
            {
                return referencedBundles
                    .Select(urlGenerator.CreateBundleUrl);
            }
        }

        class AssetCollector : IBundleVisitor
        {
            public static IEnumerable<IAsset> GetAllAssets(Bundle bundle)
            {
                var collector = new AssetCollector();
                bundle.Accept(collector);
                return collector.assets;
            }

            readonly List<IAsset> assets = new List<IAsset>();

            public void Visit(Bundle bundle)
            {
            }

            public void Visit(IAsset asset)
            {
                assets.Add(asset);
            }
        }

        public string Url<T>(string bundlePath)
            where T : Bundle
        {
            using (bundles.GetReadLock())
            {
                var bundle = bundles.FindBundlesContainingPath(bundlePath).OfType<T>().FirstOrDefault();
                if (bundle == null)
                {
                    throw new ArgumentException(string.Format("Bundle not found with path \"{0}\".", bundlePath));
                }

                return urlGenerator.CreateBundleUrl(bundle);
            }
        }

        public string FileUrl(string applicationRelativeFilePath)
        {
            applicationRelativeFilePath = PathUtilities.AppRelative(applicationRelativeFilePath);

            var file = settings.SourceDirectory.GetFile(applicationRelativeFilePath);
            ThrowIfFileNotFound(applicationRelativeFilePath, file);
            ThrowIfCannotRequestRawFile(applicationRelativeFilePath, file);

            using (var stream = file.OpenRead())
            {
                var hash = stream.ComputeSHA1Hash().ToHexString();
                return urlGenerator.CreateRawFileUrl(applicationRelativeFilePath, hash);
            }
        }

        void ThrowIfCannotRequestRawFile(string applicationRelativeFilePath, IFile file)
        {
            if (fileAccessAuthorization.CanAccess(file.FullPath)) return;

            throw new Exception(
                string.Format(
                    "The file {0} cannot be requested. Implement {1} to configure which files are safe to request.",
                    applicationRelativeFilePath,
                    typeof(IConfiguration<IFileAccessAuthorization>).FullName
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