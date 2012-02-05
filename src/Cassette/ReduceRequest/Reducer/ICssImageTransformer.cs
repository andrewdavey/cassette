using System.Collections.Generic;

namespace Cassette.ReduceRequest.Reducer
{
    public interface ICssImageTransformer
    {
        IEnumerable<BackgroundImageClass> ExtractImageUrls(string cssContent);
        string InjectSprite(string originalCss, SpritedImage image);
    }
}