using System;

namespace Cassette.Spriting
{
    public class DefaultSpritingSettingsConfiguration : IConfiguration<SpritingSettings>
    {
        public void Configure(SpritingSettings settings)
        {
            // Defaults copied from RequestReduce's RRConfiguration.
            settings.SpriteColorLimit = 5000;
            settings.SpriteSizeLimit = 50000;
            settings.ImageOptimizationCompressionLevel = 5;
            settings.ShouldSpriteImage = IsLocalPngUrl;
        }

        static bool IsLocalPngUrl(string imageUrl)
        {
            return imageUrl.StartsWith("/") 
                && imageUrl.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
        }
    }
}
