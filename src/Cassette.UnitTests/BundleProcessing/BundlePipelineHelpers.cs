using Should;
using Xunit;

namespace Cassette.BundleProcessing
{
    public class BundlePipelineHelpers_Tests
    {
        public class IndexOfType_Tests
        {
            [Fact]
            public void IndexOfTypeNotInPipelineReturnsNegativeOne()
            {
                var pipeline = new TestableBundlePipeline();
                var index = pipeline.IndexOf<AssignHash>();
                index.ShouldEqual(-1);
            }

            [Fact]
            public void IndexOfFirstTypeInPipelineEquals0()
            {
                var pipeline = new TestableBundlePipeline();
                pipeline.Add(new AssignHash());
                var index = pipeline.IndexOf<AssignHash>();
                index.ShouldEqual(0);
            }

            [Fact]
            public void IndexOfSecondTypeInPipelineEquals1()
            {
                var pipeline = new TestableBundlePipeline();
                pipeline.Add(new AssignHash());
                pipeline.Add(new AssignContentType("text/javascript"));
                var index = pipeline.IndexOf<AssignContentType>();
                index.ShouldEqual(1);
            }
        }
    }
}