using System.Collections.Generic;

namespace Cassette.BundleProcessing
{
    public interface IBundlePipeline<T> : IList<IBundleProcessor<T>>, IBundleProcessor<T>
        where T : Bundle
    {
    }
}