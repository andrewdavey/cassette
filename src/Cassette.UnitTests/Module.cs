using Should;
using Xunit;
using Moq;
using System;

namespace Cassette
{
    public class Module_Tests
    {
        [Fact]
        public void DisposeDisposesAllDisposableAssets()
        {
            var module = new Module("c:\\");
            var asset1 = new Mock<IDisposable>();
            var asset2 = new Mock<IDisposable>();
            var asset3 = new Mock<IAsset>(); // Not disposable; Tests for incorrect casting to IDisposable.
            module.Assets.Add(asset1.As<IAsset>().Object);
            module.Assets.Add(asset2.As<IAsset>().Object);
            module.Assets.Add(asset3.Object);

            module.Dispose();

            asset1.Verify(a => a.Dispose());
            asset2.Verify(a => a.Dispose());
        }
    }
}
