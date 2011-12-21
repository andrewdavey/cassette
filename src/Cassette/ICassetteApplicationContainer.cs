using System;

namespace Cassette
{
    interface ICassetteApplicationContainer : IDisposable
    {
        ICassetteApplication Application { get; }
        void RecycleApplication();
    }
}