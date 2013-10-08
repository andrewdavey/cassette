using System.IO;
using Cassette.BundleProcessing;
using Cassette.TinyIoC;
using Moq;
using Xunit;

namespace Cassette.Scripts
{
    public class GivenPipeline
    {
        readonly ScriptBundle bundle;
        readonly Mock<IAsset> asset;
        readonly IBundlePipeline<ScriptBundle> pipeline;

        public GivenPipeline()
        {
            bundle = new ScriptBundle("~/");
            asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/test.js");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            bundle.Assets.Add(asset.Object);

            var settings = new CassetteSettings();
            var container = new TinyIoCContainer();
            container.Register(Mock.Of<IJavaScriptMinifier>());
            container.Register(Mock.Of<IUrlGenerator>());
            container.Register(settings);

            pipeline = new ScriptPipeline(container, settings);
        }

        [Fact]
        public void WhenModifyPipeline_ThenParseJavaScriptNotTypeScriptReferencesReplacesParseJavaScriptReferences()
        {
            var modifier = new TypeScriptBundlePipelineModifier();

            // We have no ParseJavaScriptNotTypeScriptReferences
            Assert.Equal(-1, pipeline.IndexOf<ParseJavaScriptNotTypeScriptReferences>());
            
            var modifiedPipeline = modifier.Modify(pipeline);

            // We have ParseJavaScriptNotTypeScriptReferences, both in the returned pipeline and the passed in pipeline (one and the same)
            Assert.Equal(1, pipeline.IndexOf<ParseJavaScriptNotTypeScriptReferences>());
            Assert.Equal(1, modifiedPipeline.IndexOf<ParseJavaScriptNotTypeScriptReferences>());
        }
    }
}