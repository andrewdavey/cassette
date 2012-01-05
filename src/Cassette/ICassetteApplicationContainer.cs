using System;

namespace Cassette
{
    public interface ICassetteApplicationContainer : IDisposable
    {
        ICassetteApplication Application { get; }
        void RecycleApplication();
    }
}