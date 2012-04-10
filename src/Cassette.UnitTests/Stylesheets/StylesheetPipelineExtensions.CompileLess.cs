using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
	public class StylesheetPipelineExtensions_CompileLess_Tests
	{
        [Fact]
        public void CompileLessReturnsTheSamePipeline()
        {
            var minifier = Mock.Of<IStylesheetMinifier>();
            var urlGenerator = Mock.Of<IUrlGenerator>();
            var pipeline = new StylesheetPipeline(minifier, urlGenerator);
            var returnedPipeline = pipeline.CompileLess();
            returnedPipeline.ShouldBeSameAs(pipeline);
        }
	}
}