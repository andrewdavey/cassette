#if !NET35
using Cassette.BundleProcessing;
using Cassette.TinyIoC;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class SassBundlePipelineModifier_Tests
    {
        readonly StylesheetPipeline originalPipeline;
        readonly IBundlePipeline<StylesheetBundle> modifiedPipeline;

        public SassBundlePipelineModifier_Tests()
        {
            var minifier = Mock.Of<IStylesheetMinifier>();
            var urlGenerator = Mock.Of<IUrlGenerator>();
            var compiler = new Mock<ISassCompiler>();
            var settings = new CassetteSettings();
            var modifier = new SassBundlePipelineModifier();

            var container = new TinyIoCContainer();
            container.Register(compiler.Object);
            container.Register(minifier);
            container.Register(urlGenerator);
            container.Register(settings);

            originalPipeline = new StylesheetPipeline(container, settings);
            modifiedPipeline = modifier.Modify(originalPipeline);
        }

        [Fact]
        public void ModifiedPipelineIsSameObjectAsOriginalPipeline()
        {
            modifiedPipeline.ShouldBeSameAs(originalPipeline);
        }

        [Fact]
        public void WhenModifiedPipelineProcessesBundle_ThenReferenceInScssAssetIsParsed()
        {
            var asset = StubAsset("~/file.scss", "// @reference 'other.scss';");
            var bundle = StubBundle(asset);

            modifiedPipeline.Process(bundle);

            asset.Verify(a => a.AddReference("other.scss", 1));
        }

        [Fact]
        public void WhenModifiedPipelineProcessesBundle_ThenReferenceInSassAssetIsParsed()
        {
            var asset = StubAsset("~/file.sass", "// @reference 'other.sass';");
            var bundle = StubBundle(asset);

            modifiedPipeline.Process(bundle);

            asset.Verify(a => a.AddReference("other.sass", 1));
        }

        [Fact]
        public void WhenModifiedPipelineProcessesBundle_ThenScssAssetHasCompileAssetTransformAdded()
        {
            var asset = StubAsset("~/file.scss");
            var bundle = StubBundle(asset);

            modifiedPipeline.Process(bundle);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is CompileAsset)));
        }

        [Fact]
        public void WhenModifiedPipelineProcessesBundle_ThenSassAssetHasCompileAssetTransformAdded()
        {
            var asset = StubAsset("~/file.sass");
            var bundle = StubBundle(asset);

            modifiedPipeline.Process(bundle);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is CompileAsset)));
        }

        Mock<IAsset> StubAsset(string filename, string content = "")
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns(filename);
            asset.Setup(a => a.GetTransformedContent()).Returns(content);
            return asset;
        }

        StylesheetBundle StubBundle(Mock<IAsset> asset)
        {
            var bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);
            return bundle;
        }
    }
}
#endif