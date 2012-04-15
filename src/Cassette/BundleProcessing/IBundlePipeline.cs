using System;
using System.Collections.Generic;

namespace Cassette.BundleProcessing
{
    public interface IBundlePipeline<T> : IList<IBundleProcessor<T>>, IBundleProcessor<T>
        where T : Bundle
    {
        void Insert<TBundleProcessor>(int index) where TBundleProcessor : class, IBundleProcessor<T>;
        void Insert<TBundleProcessorFactory>(int index, Func<TBundleProcessorFactory, IBundleProcessor<T>> create) where TBundleProcessorFactory : class;
    }
}