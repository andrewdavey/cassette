using Cassette.Configuration;

namespace Cassette.BundleProcessing
{
    public class SortAssetsByDependency : IBundleProcessor<Bundle>
    {
        public void Process(Bundle bundle, CassetteSettings settings)
        {
            bundle.SortAssetsByDependency();
        }
    }
}
