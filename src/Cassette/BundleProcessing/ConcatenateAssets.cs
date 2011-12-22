using Cassette.Configuration;

namespace Cassette.BundleProcessing
{
    public class ConcatenateAssets : IBundleProcessor<Bundle>
    {
        public void Process(Bundle bundle, CassetteSettings settings)
        {
            bundle.ConcatenateAssets();
        }
    }
}
