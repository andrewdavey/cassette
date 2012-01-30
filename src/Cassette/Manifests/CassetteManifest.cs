using System.Collections.Generic;
using System.Linq;

namespace Cassette.Manifests
{
    class CassetteManifest
    {
        public CassetteManifest()
        {
            BundleManifests = new List<BundleManifest>();
        }

        public IList<BundleManifest> BundleManifests { get; private set; }

        #pragma warning disable 659
        // There is no sensible GetHashCode for this object because the BundlesManifests list could mutate.

        public override bool Equals(object obj)
        {
            var other = obj as CassetteManifest;
            return other != null && BundleManifests.SequenceEqual(other.BundleManifests);
        }

        #pragma warning restore 659
    }
}