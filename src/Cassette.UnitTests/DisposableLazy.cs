using System;
using Cassette.Utilities;
using Moq;
using Xunit;

namespace Cassette
{
    public class DisposableLazy_Tests
    {
        [Fact]
        public void WhenDispose_ThenValueIsDisposed()
        {
            var disposable = new Mock<IDisposable>();
            var lazy = new DisposableLazy<IDisposable>(() => disposable.Object);
// ReSharper disable UnusedVariable
            var temp = lazy.Value; // force value to be created.
// ReSharper restore UnusedVariable
            
            lazy.Dispose();

            disposable.Verify(d => d.Dispose());
        }
    }
}