using System.IO;
using System.Linq;
using Cassette.Utilities;
using Moq;
using Should;
using TinyIoC;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetPipeline_Tests
    {
        readonly Mock<IStylesheetMinifier> minifier;
        readonly Mock<IUrlGenerator> urlGenerator;
        readonly CassetteSettings settings;
        readonly TinyIoCContainer container;

        public StylesheetPipeline_Tests()
        {
            minifier = new Mock<IStylesheetMinifier>();
            urlGenerator = new Mock<IUrlGenerator>();
            settings = new CassetteSettings
            {
                SourceDirectory = new FakeFileSystem()
            };
            container = new TinyIoCContainer();
            container.Register(minifier.Object);
            container.Register(urlGenerator.Object);
            container.Register(settings);
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
            var pipeline = new StylesheetPipeline(container, settings);
            pipeline.RemoveAt(pipeline.Count - 2);

            pipeline.Process(bundle);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t == minifier.Object)));
        }

        [Fact]
        public void GivenPipelineWithEmbedImages_WhenProcessBundle_ThenLessAssetHasDataUriTransformAdded()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/file.less");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            var bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);

            var pipeline = new StylesheetPipeline(container, settings);
            bundle.Pipeline = pipeline;
            bundle.EmbedImages();
            pipeline.Process(bundle);

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

            var pipeline = new StylesheetPipeline(container, settings);
            pipeline.Process(bundle);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is CssImageToDataUriTransformer)), Times.Never());
        }

        [Fact]
        public void WhenProcessBundle_ThenHashIsAssigned()
        {
            var bundle = new StylesheetBundle("~");

            var pipeline = new StylesheetPipeline(container, settings);
            pipeline.Process(bundle);

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

            minifier = new MicrosoftStylesheetMinifier();
            urlGenerator = new Mock<IUrlGenerator>();
            container = new TinyIoCContainer();
            container.Register(minifier);
            container.Register(urlGenerator.Object);
            container.Register(settings);
        }

        readonly IStylesheetMinifier minifier;
        readonly Mock<IUrlGenerator> urlGenerator;
        protected readonly Mock<IAsset> asset1;
        protected readonly Mock<IAsset> asset2;
        protected readonly StylesheetBundle bundle;
        protected readonly CassetteSettings settings;
        protected readonly TinyIoCContainer container;
    }

    public class StylesheetPipeline_Process_Tests : StylesheetPipeline_Process_TestBase
    {
        [Fact]
        public void CssReferencesAreParsed()
        {
            var pipeline = new StylesheetPipeline(container, settings);
            pipeline.Process(bundle);
            asset1.Verify(a => a.AddReference("asset2.css", 1));
        }

        [Fact]
        public void GivenDebugMode_ThenCssUrlsAreExpanded()
        {
            settings.IsDebuggingEnabled = false;
            var pipeline = new StylesheetPipeline(container, settings);
            pipeline.Process(bundle);
            asset2.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(
                transformer => transformer is ExpandCssUrlsAssetTransformer)
            ));
        }

        [Fact]
        public void AssetsAreSortedByDependency()
        {
            settings.IsDebuggingEnabled = true;
            var pipeline = new StylesheetPipeline(container, settings);
            pipeline.Process(bundle);
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
            var pipeline = new StylesheetPipeline(container, settings);
            pipeline.Process(bundle);

            bundle.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void AssetsAreMinified()
        {
            var pipeline = new StylesheetPipeline(container, settings);
            pipeline.Process(bundle);

            using (var reader = new StreamReader(bundle.Assets[0].OpenStream()))
            {
                reader.ReadToEnd().ShouldEqual("p{color:#fff}");
            }
        }
    }
}