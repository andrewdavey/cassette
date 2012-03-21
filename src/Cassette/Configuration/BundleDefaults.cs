using Cassette.BundleProcessing;

namespace Cassette.Configuration
{
    interface IBundleDefaults
    {
        IBundleFactory<Bundle> BundleFactory { get; } 
    }

    public class BundleDefaults<T> : IBundleDefaults
        where T : Bundle
    {
        internal IBundleFactory<T> BundleFactory { get; set; }
        public IBundlePipeline<T> BundlePipeline { get; set; }
        public FileSearch FileSearch { get; set; }

        IBundleFactory<Bundle> IBundleDefaults.BundleFactory
        {
            get { return BundleFactory; }
        }
    }
}