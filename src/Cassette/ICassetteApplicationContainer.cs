using System;

namespace Cassette
{
    interface ICassetteApplicationContainer<out T> : IDisposable
        where T : ICassetteApplication
    {
        T Application { get; }
        void RecycleApplication();
    }
}