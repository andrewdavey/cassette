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
        }
    }
}
