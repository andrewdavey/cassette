#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.IO;
using System.Linq;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.ModuleProcessing
{
    public class ConcatenateAssets_Tests
    {
        [Fact]
        public void GivenModuleWithTwoAssets_WhenConcatenateAssetsProcessesModule_ThenASingleAssetReplacesTheTwoOriginalAssets()
        {
            var module = new Module("~");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            asset1.Setup(a => a.OpenStream()).Returns(() => ("asset1" + Environment.NewLine + "content").AsStream());
            asset2.Setup(a => a.OpenStream()).Returns(() => ("asset2" + Environment.NewLine + "content").AsStream());
            module.Assets.Add(asset1.Object);
            module.Assets.Add(asset2.Object);

            var processor = new ConcatenateAssets();
            processor.Process(module, Mock.Of<ICassetteApplication>());

            module.Assets.Count.ShouldEqual(1);
            using (var reader = new StreamReader(module.Assets[0].OpenStream()))
            {
                reader.ReadToEnd().ShouldEqual("asset1" + Environment.NewLine + "content" + Environment.NewLine + "asset2" + Environment.NewLine + "content");
            }
            (module.Assets[0] as IDisposable).Dispose();
        }

        [Fact]
        public void ConcatenateAssetsMergesAssetReferences()
        {
            var module = new Module("~");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            asset1.Setup(a => a.OpenStream()).Returns(() => "asset1".AsStream());
            asset1.SetupGet(a => a.References).Returns(new[] 
            {
                new AssetReference("~\\other1.js", asset1.Object, 0, AssetReferenceType.DifferentModule)
            });
            asset2.Setup(a => a.OpenStream()).Returns(() => "asset2".AsStream());
            asset2.SetupGet(a => a.References).Returns(new[]
            { 
                new AssetReference("~\\other1.js", asset2.Object, 0, AssetReferenceType.DifferentModule),
                new AssetReference("~\\other2.js", asset2.Object, 0, AssetReferenceType.DifferentModule) 
            });
            module.Assets.Add(asset1.Object);
            module.Assets.Add(asset2.Object);

            var processor = new ConcatenateAssets();
            processor.Process(module, Mock.Of<ICassetteApplication>());

            module.Assets[0].References
                .Select(r => r.Path)
                .OrderBy(f => f)
                .SequenceEqual(new[] { "~\\other1.js", "~\\other1.js", "~\\other2.js" })
                .ShouldBeTrue();
        }
    }
}

