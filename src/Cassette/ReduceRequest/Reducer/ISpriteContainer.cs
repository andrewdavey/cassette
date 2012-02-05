using System.Collections.Generic;

namespace Cassette.ReduceRequest.Reducer
{
    public interface ISpriteContainer : IEnumerable<SpritedImage>
    {
        SpritedImage AddImage(BackgroundImageClass image);
        void AddImage(SpritedImage image);
        int Size { get; }
        int Colors { get; }
        int Width { get; }
        int Height { get; }
    }
}