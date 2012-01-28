using System;
using System.Collections.Generic;
using Cassette.IO;
using Cassette.Scripts;
using Moq;
using Xunit;

namespace Cassette
{
    class RuntimeBundleContainerFactory_Tests
    {
        [Fact]
        public void _()
        {
            var bundle = new ScriptBundle("~/path");
            bundle.Assets.Add(StubAsset("~/asset"));
            var bundlesFromConfiguration = new[] { bundle };

            using (var cachePath = new TempDirectory())
            using (var sourcePath = new TempDirectory())
            {
                var cacheDirectory = new FileSystemDirectory(cachePath);
                var sourceDirectory = new FileSystemDirectory(sourcePath);

                var manifestXml = @"<Bundles><ScriptBundle Path=""~/path""><Asset Path=""~/asset""/></ScriptBundle></Bundles>";

                var factory = new RuntimeBundleContainerFactory(bundlesFromConfiguration);

                //var container = factory.CreateBundleContainer();
            }

            throw new NotImplementedException();
        }

        IAsset StubAsset(string path)
        {
            var asset = new Mock<IAsset>();
            return asset.Object;
        }
    }

    class RuntimeBundleContainerFactory
    {
        public RuntimeBundleContainerFactory(IEnumerable<Bundle> bundlesFromConfiguration)
        {
        }
    }
}
