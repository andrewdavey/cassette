using System;

namespace Cassette
{
    class DisposableLazy<T> : Lazy<T>, IDisposable
        where T : IDisposable
    {
        public DisposableLazy(Func<T> valueFactory)
            : base(valueFactory)
        {
        }

        public void Dispose()
        {
            if (IsValueCreated)
            {
                Value.Dispose();
            }
        }
    }
}