#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cassette.Utilities;

namespace Cassette.UI
{
    public class ReferenceBuilder : IReferenceBuilder
    {
        public ReferenceBuilder(IBundleContainer bundleContainer, IDictionary<Type, IBundleFactory<Bundle>> bundleFactories, IPlaceholderTracker placeholderTracker, ICassetteApplication application)
        {
            this.bundleContainer = bundleContainer;
            this.bundleFactories = bundleFactories;
            this.placeholderTracker = placeholderTracker;
            this.application = application;
        }

        readonly IBundleContainer bundleContainer;
        readonly IDictionary<Type, IBundleFactory<Bundle>> bundleFactories;
        readonly IPlaceholderTracker placeholderTracker;
        readonly ICassetteApplication application;
        readonly Dictionary<string, List<Bundle>> bundlesByLocation = new Dictionary<string, List<Bundle>>();
        readonly HashSet<string> renderedLocations = new HashSet<string>();
 
        // TODO: Add typed Reference<TBundle> method

        public void Reference(string path, string location = null)
        {
            path = PathUtilities.AppRelative(path);

            var bundle = bundleContainer.FindBundleContainingPath(path);
            if (bundle == null && path.IsUrl())
            {
                // Ad-hoc external bundle reference.
                // TODO: Attempt to determine bundle type from URL.
                bundle = bundleFactories[typeof(Scripts.ScriptBundle)].CreateBundle(path, null);
            }

            if (bundle == null)
            {
                throw new ArgumentException("Cannot find an asset bundle containing the path \"" + path + "\".");                
            }

            // Bundle can define it's own prefered location. Use this when we aren't given
            // an explicit location argument i.e. null.
            if (location == null)
            {
                location = bundle.Location;
            }

            Reference(bundle, location);
        }

        public void Reference(Bundle bundle, string location = null)
        {
            if (!application.IsHtmlRewritingEnabled && HasRenderedLocation(location))
            {
                ThrowRewritingRequiredException(location);
            }

            var bundles = GetOrCreateBundleSet(location);
            if (bundles.Contains(bundle)) return;
            bundles.Add(bundle);
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
            return bundleContainer.IncludeReferencesAndSortBundles(bundles);
        }

        public IHtmlString Render<T>(string location = null)
            where T : Bundle
        {
            renderedLocations.Add(location ?? "");
            return placeholderTracker.InsertPlaceholder(
                () => CreateHtml<T>(location)
            );
        }

        public string BundleUrl(string path)
        {
            var bundle = bundleContainer.FindBundleContainingPath(path);
            if (bundle == null)
            {
                throw new ArgumentException("Cannot find bundle contain path \"" + path + "\".");
            }
            return application.UrlGenerator.CreateBundleUrl(bundle);
        }

        HtmlString CreateHtml<T>(string location)
            where T : Bundle
        {
            return new HtmlString(string.Join(Environment.NewLine,
                GetBundles(location).OfType<T>().Select(
                    bundle => bundle.Render(application).ToHtmlString()
                )
            ));
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