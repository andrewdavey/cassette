namespace Cassette.Spriting.Spritastic
{
    interface ISpritingSettings
    {
        int SpriteSizeLimit { get; set; }
        int SpriteColorLimit { get; set; }
        bool ImageQuantizationDisabled { get; set; }
        bool ImageOptimizationDisabled { get; set; }
        int ImageOptimizationCompressionLevel { get; set; }
        bool IsFullTrust { get; }
    }
}