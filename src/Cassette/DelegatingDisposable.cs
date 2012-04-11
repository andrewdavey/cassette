using System;

namespace Cassette
{
    class DelegatingDisposable : IDisposable
    {
        readonly Action dispose;

        public DelegatingDisposable(Action dispose)
        {
            this.dispose = dispose;
        }

        public void Dispose()
        {
            dispose();
        }
    }
}