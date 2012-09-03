using System.Collections.Generic;
using Cassette.Spriting.Spritastic.ImageLoad;
using Cassette.Spriting.Spritastic.Parser;

namespace Cassette.Spriting.Spritastic.Generator
{
    interface ISpriteContainer : IEnumerable<SpritedImage>
    {
        SpritedImage AddImage (BackgroundImageClass image);
        void AddImage(SpritedImage image);
        int Size { get; }
        int Colors { get; }
        int Width { get; }
        int Height { get; }
        IImageLoader ImageLoader { get; set; }
    }
}