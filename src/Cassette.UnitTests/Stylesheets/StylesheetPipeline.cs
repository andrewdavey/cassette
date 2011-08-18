using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Should;
using Moq;
using Cassette.Utilities;
using System.IO;
using Cassette.ModuleProcessing;

namespace Cassette.Stylesheets
{
    public class StylesheetPipeline_Tests
    {
        [Fact]
        public void CompileLessDefaultsToFalse()
        {
            new StylesheetPipeline().CompileLess.ShouldBeFalse();
        }

        [Fact]
        public void StylesheetMinifierDefaultsToMicrosoft()
        {
            new StylesheetPipeline().StylesheetMinifier.ShouldBeType<MicrosoftStyleSheetMinifier>();
        }
    }

    public class StylesheetPipeline_Process_TestBase
    {
        public StylesheetPipeline_Process_TestBase()
        {
            application = new Mock<ICassetteApplication>();            
            module = new StylesheetModule("");
            asset1 = new Mock<IAsset>();
            asset2 = new Mock<IAsset>();
            module.Assets.Add(asset1.Object);
            module.Assets.Add(asset2.Object);

            asset1.SetupGet(a => a.SourceFilename)
                  .Returns("asset1.css");
            asset1.Setup(a => a.OpenStream())
                  .Returns(() => "/* @reference \"asset2.css\"; */".AsStream());
            asset2.SetupGet(a => a.SourceFilename)
                  .Returns("asset2.css");
            asset2.Setup(a => a.OpenStream())
                  .Returns(() => "p { color: White; }".AsStream());
            asset1.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("asset2.css", asset1.Object, -1, AssetReferenceType.SameModule) });
        }

        protected readonly Mock<IAsset> asset1;
        protected readonly Mock<IAsset> asset2;
        protected readonly StylesheetModule module;
        protected readonly Mock<ICassetteApplication> application;
    }

    public class StylesheetPipeline_Process_Tests : StylesheetPipeline_Process_TestBase
    {
        [Fact]
        public void CssReferencesAreParsed()
        {
            new StylesheetPipeline().Process(module, application.Object);
            asset1.Verify(a => a.AddReference("asset2.css", -1));
        }

        [Fact]
        public void CssUrlsAreExpanded()
        {
            new StylesheetPipeline().Process(module, application.Object);
            asset2.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(
                transformer => transformer is ExpandCssUrlsAssetTransformer)
            ));
        }

        [Fact]
        public void AssetsAreSortedByDependency()
        {
            new StylesheetPipeline().Process(module, application.Object);
            module.Assets.SequenceEqual(new[] { asset2.Object, asset1.Object }).ShouldBeTrue();
        }
    }

    public class StylesheetPipelineWhereApplicationIsOutputOptimized : StylesheetPipeline_Process_TestBase
    {
        public StylesheetPipelineWhereApplicationIsOutputOptimized()
        {
            application.Setup(a => a.IsOutputOptimized).Returns(true);
        }

        [Fact]
        public void AssetsAreConcatenated()
        {
            new StylesheetPipeline().Process(module, application.Object);
            module.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void AssetsAreMinified()
        {
            var pipeline = new StylesheetPipeline();

            pipeline.Process(module, application.Object);

            using (var reader = new StreamReader(module.Assets[0].OpenStream()))
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
            asset.SetupGet(a => a.SourceFilename).Returns("asset.less");
            asset.Setup(a => a.OpenStream()).Returns(() => "// @reference 'other.less';".AsStream());
            module.Assets.Add(asset.Object);
            pipeline = new StylesheetPipeline();
            pipeline.CompileLess = true;
        }

        readonly Mock<IAsset> asset;
        readonly StylesheetPipeline pipeline;

        [Fact]
        public void ReferenceInLessAssetIsParsed()
        {
            pipeline.Process(module, application.Object);
            asset.Verify(a => a.AddReference("other.less", 1));
        }

        [Fact]
        public void LessAssetIsCompiled()
        {
            pipeline.Process(module, application.Object);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(
                t => t is CompileAsset
            )));
        }
    }
}