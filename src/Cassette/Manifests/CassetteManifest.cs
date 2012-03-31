using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.IO;
using Cassette.Configuration;

namespace Cassette.Manifests
{
    // There is no sensible GetHashCode for this object because the BundlesManifests list could mutate.
#pragma warning disable 659
    class CassetteManifest
#pragma warning restore 659
    {
        public CassetteManifest()
        {
            Version = "";
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

        public bool IsUpToDateWithFileSystem(IDirectory directory)
        {
            return BundleManifests.All(
                bundleManifest => bundleManifest.IsUpToDateWithFileSystem(directory, LastWriteTimeUtc)
            );
        }

        public IEnumerable<Bundle> CreateBundles(IUrlModifier urlModifier)
        {
            return BundleManifests.Select(m => m.CreateBundle(urlModifier));
        }

// ReSharper disable CSharpWarnings::CS0659
        public override bool Equals(object obj)
// ReSharper restore CSharpWarnings::CS0659
        {
            var other = obj as CassetteManifest;
            return other != null 
                && Version == other.Version 
                && BundleManifestsEqual(other);
        }

        bool BundleManifestsEqual(CassetteManifest other)
        {
            return BundleManifests.OrderBy(b => b.Path).SequenceEqual(other.BundleManifests.OrderBy(b => b.Path));
        }
    }
}