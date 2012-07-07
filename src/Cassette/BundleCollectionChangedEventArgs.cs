using System;
using System.Collections.Generic;

namespace Cassette
{
    public class BundleCollectionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a readonly copy of the updated bundle collection.
        /// </summary>
        public IEnumerable<Bundle> Bundles { get; private set; }

        public BundleCollectionChangedEventArgs(IEnumerable<Bundle> bundles)
        {
            Bundles = bundles;
        }
    }
}