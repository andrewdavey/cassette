using System;

namespace Cassette
{
    public interface ICassetteApplicationContainer<out T> : IDisposable
        where T : ICassetteApplication
    {
        T Application { get; }
        void RecycleApplication();
    }
}