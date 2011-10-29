using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cassette.Utilities;

namespace Cassette.Configuration
{
    public class BundleCollection : IEnumerable<Bundle>
    {
        readonly CassetteSettings settings;
        readonly Dictionary<string, List<Bundle>> bundlesByPath = new Dictionary<string, List<Bundle>>();

        public BundleCollection(CassetteSettings settings)
        {
            this.settings = settings;
        }

        internal CassetteSettings Settings
        {
            get { return settings; }
        }

        public void Add(Bundle bundle)
        {
            var bundles = GetOrCreateBundleList(bundle.Path);
            var bundleTypeAlreadyAdded = bundles.Any(b => b.GetType() == bundle.GetType());
            if (bundleTypeAlreadyAdded)
            {
                throw new ArgumentException(
                    string.Format("A bundle with the path \"{0}\" has already been added to the collection.", bundle.Path)
                );
            }
            bundles.Add(bundle);
        }

        public T Get<T>(string path)
            where T : Bundle
        {
            return Get(path, bundles => bundles.OfType<T>().First());
        }

        public Bundle Get(string path)
        {
            return Get(path, bundles => bundles[0]);
        }

        public Bundle this[string path]
        {
            get { return Get(path); }
        }

        public IEnumerator<Bundle> GetEnumerator()
        {
            return bundlesByPath.SelectMany(kvp => kvp.Value).GetEnumerator();
        }

        internal void Remove(Bundle bundle)
        {
            var bundles = GetOrCreateBundleList(bundle.Path);
            bundles.Remove(bundle);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        T Get<T>(string path, Func<List<Bundle>, T> getFromList)
        {
            path = PathUtilities.AppRelative(path);
            List<Bundle> bundles;
            if (bundlesByPath.TryGetValue(path, out bundles))
            {
                return getFromList(bundles);
            }

            throw new ArgumentException(
                string.Format("Bundle not found with path \"{0}\".", path)
            );
        }

        List<Bundle> GetOrCreateBundleList(string path)
        {
            List<Bundle> bundles;
            if (!bundlesByPath.TryGetValue(path, out bundles))
            {
                bundles = new List<Bundle>();
                bundlesByPath[path] = bundles;
            }

            return bundles;
        }
    }
}