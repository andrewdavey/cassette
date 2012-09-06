using System;

namespace Cassette
{
    class BundleConfiguration : IConfiguration<BundleCollection>
    {
        readonly Action<BundleCollection> addBundles;

        public BundleConfiguration(Action<BundleCollection> addBundles)
        {
            this.addBundles = addBundles;
        }

        public void Configure(BundleCollection bundles)
        {
            addBundles(bundles);
        }
    }
}