using System;
using System.Collections.Generic;
using Cassette.Spriting.Spritastic.ImageLoad;
using Cassette.Spriting.Spritastic.Parser;

namespace Cassette.Spriting.Spritastic.Generator
{
    interface ISpriteManager : IEnumerable<SpritedImage>, IDisposable
    {
        void Add(BackgroundImageClass imageUrl);
        IList<Sprite> Flush();
        IImageLoader ImageLoader { get; set; }
        Predicate<BackgroundImageClass> ImageExclusionFilter { get; set; }
        IList<SpriteException> Errors { get; }
    }
}