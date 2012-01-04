using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Utilities;
using Cassette.Configuration;

namespace Cassette
{
    class ReferenceBuilder : IReferenceBuilder
    {
        public ReferenceBuilder(IBundleContainer bundleContainer, IDictionary<Type, IBundleFactory<Bundle>> bundleFactories, IPlaceholderTracker placeholderTracker, CassetteSettings settings)
        {
            this.bundleContainer = bundleContainer;
            this.bundleFactories = bundleFactories;
            this.placeholderTracker = placeholderTracker;
            this.settings = settings;
        }

        readonly IBundleContainer bundleContainer;
        readonly IDictionary<Type, IBundleFactory<Bundle>> bundleFactories;
        readonly IPlaceholderTracker placeholderTracker;
        readonly CassetteSettings settings;
        readonly Dictionary<string, List<Bundle>> bundlesByLocation = new Dictionary<string, List<Bundle>>();
        readonly HashSet<string> renderedLocations = new HashSet<string>();
 
        public void Reference<T>(string path, string location = null)
            where T : Bundle
        {
            var bundles = GetBundles(path, () => bundleFactories[typeof(T)].CreateExternalBundle(path));
            Reference(bundles, location);
        }

        public void Reference(string path, string location = null)
        {
            var bundles = GetBundles(path, () =>
            {
                if (path.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                {
                    return bundleFactories[typeof(Scripts.ScriptBundle)].CreateExternalBundle(path);
                }
                else if (path.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                {
                    return bundleFactories[typeof(Stylesheets.StylesheetBundle)].CreateExternalBundle(path);
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
            });

            Reference(bundles, location);
        }

        IEnumerable<Bundle> GetBundles(string path, Func<Bundle> createExternalBundle)
        {
            path = PathUtilities.AppRelative(path);

            var bundles = bundleContainer.FindBundlesContainingPath(path).ToArray();
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
            Reference(new[] { bundle }, location);
        }

        void Reference(IEnumerable<Bundle> bundles, string location = null)
        {
            if (!settings.IsHtmlRewritingEnabled && HasRenderedLocation(location))
            {
                ThrowRewritingRequiredException(location);
            }

            foreach (var bundle in bundles)
            {
                // Bundle can define it's own prefered location. Use this when we aren't given
                // an explicit location argument i.e. null.
                if (location == null)
                {
                    location = bundle.PageLocation;
                }

                var bundlesForLocation = GetOrCreateBundleSet(location);
                if (bundlesForLocation.Contains(bundle)) return;
                bundlesForLocation.Add(bundle);
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
            var bundles = GetOrCreateBundleSet(location);
            var bundlesForLocation = GetOrCreateBundleSet(location);
            return bundleContainer.IncludeReferencesAndSortBundles(bundles).Where(b => bundlesForLocation.Contains(b) || b.PageLocation == location);
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
                )
            );
        }

        List<Bundle> GetOrCreateBundleSet(string location)
        {
            location = location ?? ""; // Dictionary doesn't accept null keys.
            List<Bundle> bundles;
            if (bundlesByLocation.TryGetValue(location, out bundles))
            {
                return bundles;
            }
            else
            {
                bundles = new List<Bundle>();
                bundlesByLocation.Add(location, bundles);
                return bundles;
            }
        }
    }
}