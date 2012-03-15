using Should;
using Xunit;

namespace Cassette.Stylesheets
{
	public class StylesheetPipelineExtensions_CompileLess_Tests
	{
        [Fact]
        public void CompileLessReturnsTheSamePipeline()
        {
            var pipeline = new StylesheetPipeline();
            var returnedPipeline = pipeline.CompileLess();
            returnedPipeline.ShouldBeSameAs(pipeline);
        }
	}
}
