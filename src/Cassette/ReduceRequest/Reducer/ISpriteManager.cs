using System;
using System.Collections.Generic;

namespace Cassette.ReduceRequest.Reducer
{
    public interface ISpriteManager : IEnumerable<SpritedImage>, IDisposable
    {
        void Add(BackgroundImageClass imageUrl);
        void Flush();
        Guid SpritedCssKey { get; set; }
    }
}