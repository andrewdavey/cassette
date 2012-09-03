using System.Collections.Generic;
using Cassette.Spriting.Spritastic.Parser;

namespace Cassette.Spriting.Spritastic.Generator
{
    interface ICssImageExtractor
    {
        IEnumerable<BackgroundImageClass> ExtractImageUrls(string cssContent);
    }
}