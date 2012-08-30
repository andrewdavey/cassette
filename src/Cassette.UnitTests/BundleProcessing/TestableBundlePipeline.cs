using Cassette.TinyIoC;

namespace Cassette.BundleProcessing
{
    class TestableBundlePipeline : BundlePipeline<TestableBundle>
    {
        public TestableBundlePipeline() : base(new TinyIoCContainer())
        {
        }

        public TestableBundlePipeline(TinyIoCContainer container)
            : base(container)
        {
        }
    }
}