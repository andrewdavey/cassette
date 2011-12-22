using System;
using System.Collections.Generic;

namespace Cassette
{
    interface IBundleContainer : IDisposable
    {
        IEnumerable<Bundle> Bundles { get; }
        T FindBundleContainingPath<T>(string path) where T : Bundle;
        IEnumerable<Bundle> IncludeReferencesAndSortBundles(IEnumerable<Bundle> bundles);
    }
}