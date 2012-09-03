namespace Cassette.Spriting.Spritastic.Utilities
{
    interface IPngOptimizer
    {
        byte[] OptimizePng(byte[] bytes, int compressionLevel, bool imageQuantizationDisabled);
    }
}