using Cassette.BundleProcessing;
using Cassette.Stylesheets;

namespace Cassette.Spriting
{
    public static class StylesheetBundleExtensions
    {
        public static void SpriteImages(this StylesheetBundle bundle)
        {
            var i = bundle.Pipeline.IndexOf<MinifyAssets>();
            bundle.Pipeline.RemoveAt(i);

            bundle.Pipeline.Insert<SpriteImages>(bundle.Pipeline.Count);
        }
    }
}