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

using System.IO;
using System.Linq;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetPipeline_Tests
    {
        [Fact]
        public void CompileLessDefaultsToTrue()
        {
            new StylesheetPipeline().CompileLess.ShouldBeTrue();
        }

        [Fact]
        public void StylesheetMinifierDefaultsToMicrosoft()
        {
            new StylesheetPipeline().StylesheetMinifier.ShouldBeType<MicrosoftStyleSheetMinifier>();
        }

        [Fact]
        public void GivenCompileLessIsTrue_WhenProcessBundle_ThenLessAssetHasCompileAssetTransformAdded()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/file.less");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            var bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);

            var pipeline = new StylesheetPipeline { CompileLess = true };
            pipeline.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is CompileAsset)));
        }

        [Fact]
        public void GivenCompileLessIsFalse_WhenProcessBundle_ThenLessAssetHasNoCompileAssetTransformAdded()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/file.less");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            var bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);

            var pipeline = new StylesheetPipeline { CompileLess = false };
            pipeline.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is CompileAsset)), Times.Never());
        }

        [Fact]
        public void GivenConvertImageUrlsToDataUrisIsTrue_WhenProcessBundle_ThenLessAssetHasDataUriTransformAdded()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/file.less");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            var bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);

            var pipeline = new StylesheetPipeline { ConvertImageUrlsToDataUris = true };
            pipeline.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is CssImageToDataUriTransformer)));
        }

        [Fact]
        public void GivenConvertImageUrlsToDataUrisIsFalse_WhenProcessBundle_ThenLessAssetHasNoDataUriTransformAdded()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/file.less");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            var bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);

            var pipeline = new StylesheetPipeline { ConvertImageUrlsToDataUris = false };
            pipeline.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is CssImageToDataUriTransformer)), Times.Never());
        }
    }

    public class StylesheetPipeline_Process_TestBase
    {
        public StylesheetPipeline_Process_TestBase()
        {
            settings = new CassetteSettings();
            bundle = new StylesheetBundle("~");
            asset1 = new Mock<IAsset>();
            asset2 = new Mock<IAsset>();
            bundle.Assets.Add(asset1.Object);
            bundle.Assets.Add(asset2.Object);

            asset1.SetupGet(a => a.SourceFile.FullPath)
                  .Returns("~/asset1.css");
            asset1.Setup(a => a.OpenStream())
                  .Returns(() => "/* @reference \"asset2.css\"; */".AsStream());
            asset2.SetupGet(a => a.SourceFile.FullPath)
                  .Returns("~/asset2.css");
            asset2.Setup(a => a.OpenStream())
                  .Returns(() => "p { color: White; }".AsStream());
            asset1.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/asset2.css", asset1.Object, -1, AssetReferenceType.SameBundle) });
        }

        protected readonly Mock<IAsset> asset1;
        protected readonly Mock<IAsset> asset2;
        protected readonly StylesheetBundle bundle;
        protected readonly CassetteSettings settings;
    }

    public class StylesheetPipeline_Process_Tests : StylesheetPipeline_Process_TestBase
    {
        [Fact]
        public void CssReferencesAreParsed()
        {
            new StylesheetPipeline().Process(bundle, settings);
            asset1.Verify(a => a.AddReference("asset2.css", 1));
        }

        [Fact]
        public void GivenDebugMode_ThenCssUrlsAreExpanded()
        {
            settings.IsDebuggingEnabled = false;
            new StylesheetPipeline().Process(bundle, settings);
            asset2.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(
                transformer => transformer is ExpandCssUrlsAssetTransformer)
            ));
        }

        [Fact]
        public void AssetsAreSortedByDependency()
        {
            settings.IsDebuggingEnabled = true;
            new StylesheetPipeline().Process(bundle, settings);
            bundle.Assets.SequenceEqual(new[] { asset2.Object, asset1.Object }).ShouldBeTrue();
        }
    }

    public class StylesheetPipelineInDebugMode : StylesheetPipeline_Process_TestBase
    {
        public StylesheetPipelineInDebugMode()
        {
            settings.IsDebuggingEnabled = false;
        }

        [Fact]
        public void AssetsAreConcatenated()
        {
            new StylesheetPipeline().Process(bundle, settings);
            bundle.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void AssetsAreMinified()
        {
            var pipeline = new StylesheetPipeline();

            pipeline.Process(bundle, settings);

            using (var reader = new StreamReader(bundle.Assets[0].OpenStream()))
            {
                reader.ReadToEnd().ShouldEqual("p{color:#fff}");
            }
        }
    }

    public class StylesheetPipelineWhereLessCompilerTrue : StylesheetPipeline_Process_TestBase
    {
        public StylesheetPipelineWhereLessCompilerTrue()
        {
            asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("asset.less");
            asset.Setup(a => a.OpenStream()).Returns(() => "// @reference 'other.less';".AsStream());
            bundle.Assets.Add(asset.Object);
            pipeline = new StylesheetPipeline();
            pipeline.CompileLess = true;
        }

        readonly Mock<IAsset> asset;
        readonly StylesheetPipeline pipeline;

        [Fact]
        public void ReferenceInLessAssetIsParsed()
        {
            pipeline.Process(bundle, settings);
            asset.Verify(a => a.AddReference("other.less", 1));
        }

        [Fact]
        public void LessAssetIsCompiled()
        {
            pipeline.Process(bundle, settings);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(
                t => t is CompileAsset
            )));
        }
    }
}