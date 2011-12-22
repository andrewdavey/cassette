using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cassette.Utilities;

namespace Cassette.Configuration
{
    /// <summary>
    /// A collection of asset bundles.
    /// </summary>
    public class BundleCollection : IEnumerable<Bundle>
    {
        readonly CassetteSettings settings;
        readonly List<Bundle> bundles = new List<Bundle>(); 
        
        public BundleCollection(CassetteSettings settings)
        {
            this.settings = settings;
        }

        internal CassetteSettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Adds a <see cref="Bundle"/> to the collection.
        /// </summary>
        /// <param name="bundle">The bundle to add.</param>
        public void Add(Bundle bundle)
        {
            var bundleTypeAlreadyAdded = bundles.Any(
                b => b.ContainsPath(bundle.Path) 
                  && b.GetType() == bundle.GetType()
            );
            if (bundleTypeAlreadyAdded)
            {
                throw new ArgumentException(
                    string.Format("A {0} with the path \"{1}\" has already been added to the collection.", bundle.GetType().Name, bundle.Path)
                );
            }
            bundles.Add(bundle);
        }

        /// <summary>
        /// Gets a strongly-typed <see cref="Bundle"/> from the collection, by path.
        /// </summary>
        /// <typeparam name="T">The type of bundle.</typeparam>
        /// <param name="path">The bundle path to find.</param>
        /// <returns>A strongly-typed bundle</returns>
        /// <exception cref="ArgumentException">Thrown when bundle is not found.</exception>
        public T Get<T>(string path)
            where T : Bundle
        {
            return Get(path, bundleArray => bundleArray.OfType<T>().First());
        }

        /// <summary>
        /// Gets a <see cref="Bundle"/> from the collection, by path.
        /// </summary>
        /// <param name="path">The bundle path to find.</param>
        /// <returns>A bundle.</returns>
        /// <exception cref="ArgumentException">Thrown when bundle is not found.</exception>
        public Bundle Get(string path)
        {
            return Get(path, bundleArray => bundleArray[0]);
        }

        /// <summary>
        /// Gets a <see cref="Bundle"/> from the collection, by path.
        /// </summary>
        /// <returns>A bundle.</returns>
        /// <exception cref="ArgumentException">Thrown when bundle is not found.</exception>
        public Bundle this[string path]
        {
            get { return Get(path); }
        }

        IEnumerator<Bundle> IEnumerable<Bundle>.GetEnumerator()
        {
            return bundles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return bundles.GetEnumerator();
        }

        T Get<T>(string path, Func<Bundle[], T> getFromMatching)
        {
            path = PathUtilities.AppRelative(path);
            var matchingBundles = bundles.Where(b => b.ContainsPath(path)).ToArray();

            if (matchingBundles.Length == 0)
            {
                throw new ArgumentException(
                    string.Format("Bundle not found with path \"{0}\".", path)
                );
            }

            return getFromMatching(matchingBundles);
        }
    }
}
