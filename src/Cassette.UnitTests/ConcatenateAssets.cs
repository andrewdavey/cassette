using System;
using System.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ConcatenateAssets_Tests
    {
        [Fact]
        public void GivenModuleWithTwoAssets_WhenConcatenateAssetsProcessesModule_ThenASingleAssetReplacesTheTwoOriginalAssets()
        {
            var module = new Module("c:\\");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            asset1.Setup(a => a.OpenStream()).Returns(() => "asset1".AsStream());
            asset2.Setup(a => a.OpenStream()).Returns(() => "asset2".AsStream());
            module.Assets.Add(asset1.Object);
            module.Assets.Add(asset2.Object);

            var processor = new ConcatentateAssets();
            processor.Process(module);

            module.Assets.Count.ShouldEqual(1);
            using (var reader = new StreamReader(module.Assets[0].OpenStream()))
            {
                reader.ReadToEnd().ShouldEqual("asset1" + Environment.NewLine + "asset2");
            }
            (module.Assets[0] as IDisposable).Dispose();
        }
    }
}
