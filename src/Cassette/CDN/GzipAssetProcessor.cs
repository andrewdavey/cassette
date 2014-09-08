using System.Linq;
using Cassette.BundleProcessing;

namespace Cassette.CDN
{
    public class GzipAssetProcessor<T> : IBundleProcessor<T> 
        where T : Bundle, IExternalBundle, ICdnBundle
    {
        public void Process(T bundle)
        {
            if (bundle == null || !bundle.Assets.Any())
                return;

            foreach (var asset in bundle.Assets)
            {
                asset.AddAssetTransformer(new GzipAssetTransformer());
            }
        }
    }
}
