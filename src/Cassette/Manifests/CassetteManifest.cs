using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.IO;

namespace Cassette.Manifests
{
    class CassetteManifest
    {
        public CassetteManifest()
        {
            BundleManifests = new List<BundleManifest>();
        }

        public CassetteManifest(string version, IEnumerable<BundleManifest> bundleManifests)
        {
            Version = version;
            BundleManifests = bundleManifests.ToList();
        }

        public DateTime LastWriteTimeUtc { get; set; }

        public string Version { get; set; }

        public IList<BundleManifest> BundleManifests { get; private set; }

        public IEnumerable<Bundle> CreateBundles()
        {
            return BundleManifests.Select(m => m.CreateBundle());
        }

        public bool IsUpToDateWithFileSystem(IDirectory directory)
        {
            return BundleManifests.All(
                bundleManifest => bundleManifest.IsUpToDateWithFileSystem(directory, LastWriteTimeUtc)
            );
        }

        #pragma warning disable 659 // There is no sensible GetHashCode for this object because the BundlesManifests list could mutate.
        public override bool Equals(object obj)
        {
            var other = obj as CassetteManifest;
            return other != null 
                && Version == other.Version 
                && BundleManifests.SequenceEqual(other.BundleManifests);
        }
        #pragma warning restore 659
    }
}