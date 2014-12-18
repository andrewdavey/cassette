using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Utilities;

#if NET35
using Iesi.Collections.Generic;
#endif

namespace Cassette
{
    class ReferenceBuilder : IReferenceBuilder
    {
        public ReferenceBuilder(BundleCollection allBundles, IPlaceholderTracker placeholderTracker, IBundleFactoryProvider bundleFactoryProvider, CassetteSettings settings)
        {
            this.allBundles = allBundles;
            this.placeholderTracker = placeholderTracker;
            this.bundleFactoryProvider = bundleFactoryProvider;
            this.settings = settings;
        }

        readonly BundleCollection allBundles;
        readonly IPlaceholderTracker placeholderTracker;
        readonly IBundleFactoryProvider bundleFactoryProvider;
        readonly CassetteSettings settings;
        readonly OrderedDependencySet<ReferencedBundle> referencedBundles = new OrderedDependencySet<ReferencedBundle>(); 
        readonly HashedSet<string> renderedLocations = new HashedSet<string>();
 
        public void Reference<T>(string path, string location = null)
            where T : Bundle
        {
            using (allBundles.GetReadLock())
            {
                var factory = bundleFactoryProvider.GetBundleFactory<T>();
                var bundles = GetBundles(path, () => factory.CreateExternalBundle(path)).OfType<T>();
#if NET35
                Reference(bundles.Cast<Bundle>(), location);
#else
                Reference(bundles, location);
#endif
            }
        }

        public void Reference(string path, string location = null)
        {
            using (allBundles.GetReadLock())
            {
                var bundles = GetBundles(path, () => CreateExternalBundleByInferringTypeFromFileExtension(path));
                Reference(bundles, location);
            }
        }

        Bundle CreateExternalBundleByInferringTypeFromFileExtension(string path)
        {
            var pathToExamine = path.IsUrl() ? RemoveQuerystring(path) : path;
            if (pathToExamine.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
            {
                return CreateExternalScriptBundle(path);
            }
            else if (pathToExamine.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            {
                return CreateExternalStylesheetBundle(path);
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        "Cannot determine the type of bundle for the URL \"{0}\". Specify the type using the generic type parameter.",
                        path
                    )
                );
            }
        }

        string RemoveQuerystring(string url)
        {
            var index = url.IndexOf('?');
            if (index < 0) return url;
            return url.Substring(0, index);
        }

        Bundle CreateExternalScriptBundle(string path)
        {
            var factory = bundleFactoryProvider.GetBundleFactory<ScriptBundle>();
            return factory.CreateExternalBundle(path);
        }

        Bundle CreateExternalStylesheetBundle(string path)
        {
            var factory = bundleFactoryProvider.GetBundleFactory<StylesheetBundle>();
            return factory.CreateExternalBundle(path);
        }

        IEnumerable<Bundle> GetBundles(string path, Func<Bundle> createExternalBundle)
        {
            path = PathUtilities.AppRelative(path);

            var bundles = allBundles.FindBundlesContainingPath(path).ToArray();
            if (bundles.Length == 0 && path.IsUrl())
            {
                var bundle = createExternalBundle();
                bundle.Process(settings);
                bundles = new[] { bundle };
            }

            if (bundles.Length == 0)
            {
                throw new ArgumentException("Cannot find an asset bundle containing the path \"" + path + "\".");
            }

            return bundles;
        }

        public void Reference(Bundle bundle, string location = null)
        {
            using (allBundles.GetReadLock())
            {
                Reference(new[] { bundle }, location);
            }
        }

        void Reference(IEnumerable<Bundle> bundles, string location = null)
        {
            if (!settings.IsHtmlRewritingEnabled && HasRenderedLocation(location))
            {
                ThrowRewritingRequiredException(location);
            }

            foreach (var bundle in bundles)
            {
                IOrderedDependencyReceiver<ReferencedBundle> receiver;
                if (!referencedBundles.Add(new ReferencedBundle(bundle, location), out receiver)) continue; // Already present. Dependencies should already be present too.
                allBundles.CollectAllReferences(bundle, b => new ReferencedBundle(b), receiver);
            }
        }

        bool HasRenderedLocation(string location)
        {
            return renderedLocations.Contains(location ?? "");
        }

        void ThrowRewritingRequiredException(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new InvalidOperationException(
                    "Cannot add a bundle reference. The bundles have already been rendered. Either move the reference before the render call, or set ICassetteApplication.IsHtmlRewritingEnabled to true in your Cassette configuration."
                );
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Cannot add a bundle reference, for location \"{0}\". This location has already been rendered. Either move the reference before the render call, or set ICassetteApplication.IsHtmlRewritingEnabled to true in your Cassette configuration.",
                        location
                    )
                );
            }
        }

        public IEnumerable<Bundle> GetBundles(string location)
        {
            return referencedBundles.Where(r => r.PageLocation == location).Select(r => r.Bundle).ToArray();
        }

        public string Render<T>(string location = null)
            where T : Bundle
        {
            renderedLocations.Add(location ?? "");
            return placeholderTracker.InsertPlaceholder(
                () => CreateHtml<T>(location)
            );
        }

        string CreateHtml<T>(string location)
            where T : Bundle
        {
            return string.Join(Environment.NewLine,
                GetBundles(location).OfType<T>().Select(
                    bundle => bundle.Render()
                ).ToArray()
            );
        }

        class ReferencedBundle
        {
            readonly Bundle bundle;
            readonly string pageLocation;

            public ReferencedBundle(Bundle bundle, string pageLocation)
            {
                this.bundle = bundle;
                this.pageLocation = pageLocation;
            }

            public ReferencedBundle(Bundle bundle)
            {
                this.bundle = bundle;
            }

            public Bundle Bundle
            {
                get { return bundle; }
            }

            public string PageLocation
            {
                get { return pageLocation ?? bundle.PageLocation; }
            }

            public override bool Equals(object obj)
            {
                var other = obj as ReferencedBundle;
                return other != null
                    && bundle.Equals(other.bundle);
            }

            public override int GetHashCode()
            {
                return bundle.GetHashCode();
            }
        }
    }
}