using Cassette.Spriting.Spritastic;
using Cassette.Spriting.Spritastic.Utilities;

namespace Cassette.Spriting
{
    public class SpritingSettings : ISpritingSettings
    {
        public int SpriteSizeLimit { get; set; }
        public int SpriteColorLimit { get; set; }
        public bool ImageQuantizationDisabled { get; set; }
        public bool ImageOptimizationDisabled { get; set; }
        public int ImageOptimizationCompressionLevel { get; set; }
        
        bool ISpritingSettings.IsFullTrust
        {
            get { return TrustLevelChecker.IsFullTrust(); }
        }
    }
}