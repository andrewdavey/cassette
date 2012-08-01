namespace Cassette.BundleProcessing
{
    public class SortAssetsByDependency : IBundleProcessor<Bundle>
    {
        public void Process(Bundle bundle)
        {
            bundle.SortAssetsByDependency();
        }
    }
}
