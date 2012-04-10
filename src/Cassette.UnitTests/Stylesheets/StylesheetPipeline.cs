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
        readonly StylesheetPipeline pipeline;
        readonly Mock<IStylesheetMinifier> minifier;
        readonly Mock<IUrlGenerator> urlGenerator;

        public StylesheetPipeline_Tests()
        {
            minifier = new Mock<IStylesheetMinifier>();
            urlGenerator = new Mock<IUrlGenerator>();
            pipeline = new StylesheetPipeline(minifier.Object, urlGenerator.Object);
        }

        [Fact]
        public void StylesheetMinifierDefaultsToMicrosoft()
        {
            var bundle = new StylesheetBundle("~");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/asset.css");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            bundle.Assets.Add(asset.Object);

            // Remove the ConcatenateAssets step, so the transformer is added to our mock asset instead of the concatenated asset object.
            var lastStep = (ConditionalBundlePipeline<StylesheetBundle>)pipeline[pipeline.Count - 1];
            lastStep.RemoveAt(0);

            pipeline.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is MicrosoftStylesheetMinifier)));
        }

        [Fact]
        public void CanProvideMinifier()
        {
            var bundle = new StylesheetBundle("~");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/asset.css");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            bundle.Assets.Add(asset.Object);

            // Remove the ConcatenateAssets step, so the transformer is added to our mock asset instead of the concatenated asset object.
            var lastStep = (ConditionalBundlePipeline<StylesheetBundle>)pipeline[pipeline.Count - 1];
            lastStep.RemoveAt(0);

            pipeline.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t == minifier.Object)));
        }

        [Fact]
        public void GivenCompileLessIsTrue_WhenProcessBundle_ThenLessAssetHasCompileAssetTransformAdded()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/file.less");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            var bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);

            pipeline.CompileLess();
            pipeline.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is CompileAsset)));
        }

        [Fact]
        public void GivenCompileLessIsFalse_WhenProcessBundle_ThenLessAssetHasNoCompileAssetTransformAdded()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/file.less");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            var bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);

            pipeline.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is CompileAsset)), Times.Never());
        }

#if !NET35
        [Fact]
        public void GivenCompileSassIsTrue_WhenProcessBundle_ThenSassAssetHasCompileAssetTransformAdded()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/file.sass");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            var bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);

            pipeline.CompileSass();
            pipeline.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is CompileAsset)));
        }

        [Fact]
        public void GivenCompileSassIsFalse_WhenProcessBundle_ThenSassAssetHasNoCompileAssetTransformAdded()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/file.sass");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            var bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);

            pipeline.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is CompileAsset)), Times.Never());
        }

#endif

        [Fact]
        public void GivenPipelineWithEmbedImages_WhenProcessBundle_ThenLessAssetHasDataUriTransformAdded()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/file.less");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            var bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);

            pipeline.EmbedImages();
            pipeline.Process(bundle, new CassetteSettings() { SourceDirectory = new FakeFileSystem() });

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is CssImageToDataUriTransformer)));
        }

        [Fact]
        public void GivenConvertImageUrlsToDataUrisIsFalse_WhenProcessBundle_ThenLessAssetHasNoDataUriTransformAdded()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/file.less");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            var bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);

            pipeline.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is CssImageToDataUriTransformer)), Times.Never());
        }

        [Fact]
        public void WhenProcessBundle_ThenHashIsAssigned()
        {
            var bundle = new StylesheetBundle("~");

            pipeline.Process(bundle, new CassetteSettings());

            bundle.Hash.ShouldNotBeNull();
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

            asset1.SetupGet(a => a.Path)
                  .Returns("~/asset1.css");
            asset1.Setup(a => a.OpenStream())
                  .Returns(() => "/* @reference \"asset2.css\"; */".AsStream());
            asset2.SetupGet(a => a.Path)
                  .Returns("~/asset2.css");
            asset2.Setup(a => a.OpenStream())
                  .Returns(() => "p { color: White; }".AsStream());
            asset1.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/asset2.css", asset1.Object, -1, AssetReferenceType.SameBundle) });

            minifier = new Mock<IStylesheetMinifier>();
            urlGenerator = new Mock<IUrlGenerator>();
            pipeline = new StylesheetPipeline(minifier.Object, urlGenerator.Object);
        }

        protected readonly StylesheetPipeline pipeline;
        protected readonly Mock<IStylesheetMinifier> minifier;
        protected readonly Mock<IUrlGenerator> urlGenerator;
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
            pipeline.Process(bundle, settings);
            asset1.Verify(a => a.AddReference("asset2.css", 1));
        }

        [Fact]
        public void GivenDebugMode_ThenCssUrlsAreExpanded()
        {
            settings.IsDebuggingEnabled = false;
            pipeline.Process(bundle, settings);
            asset2.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(
                transformer => transformer is ExpandCssUrlsAssetTransformer)
            ));
        }

        [Fact]
        public void AssetsAreSortedByDependency()
        {
            settings.IsDebuggingEnabled = true;
            pipeline.Process(bundle, settings);
            bundle.Assets.SequenceEqual(new[] { asset2.Object, asset1.Object }).ShouldBeTrue();
        }
    }

    public class StylesheetPipelineNotInDebugMode : StylesheetPipeline_Process_TestBase
    {
        public StylesheetPipelineNotInDebugMode()
        {
            settings.IsDebuggingEnabled = false;
        }

        [Fact]
        public void AssetsAreConcatenated()
        {
            pipeline.Process(bundle, settings);

            bundle.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void AssetsAreMinified()
        {
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
            asset.SetupGet(a => a.Path).Returns("asset.less");
            asset.Setup(a => a.OpenStream()).Returns(() => "// @reference 'other.less';".AsStream());
            bundle.Assets.Add(asset.Object);
            pipeline.CompileLess();
        }

        readonly Mock<IAsset> asset;

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

#if !NET35
    public class StylesheetPipelineWithSassCompilation : StylesheetPipeline_Process_TestBase
    {
        public StylesheetPipelineWithSassCompilation()
        {
            asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("asset.scss");
            asset.Setup(a => a.OpenStream()).Returns(() => "// @reference 'other.scss';".AsStream());
            bundle.Assets.Add(asset.Object);
            pipeline.CompileSass();
        }

        readonly Mock<IAsset> asset;

        [Fact]
        public void ReferenceInSassAssetIsParsed()
        {
            pipeline.Process(bundle, settings);
            asset.Verify(a => a.AddReference("other.scss", 1));
        }

        [Fact]
        public void SassAssetIsCompiled()
        {
            pipeline.Process(bundle, settings);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(
                t => t is CompileAsset
            )));
        }
    }
#endif
}