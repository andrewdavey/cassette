using Cassette.Stylesheets;

namespace Cassette.Spriting
{
    public static class StylesheetBundleExtensions
    {
        public static void SpriteImages(this StylesheetBundle bundle)
        {
            bundle.Pipeline.Insert<SpriteImages>(bundle.Pipeline.Count);
        }
    }
}