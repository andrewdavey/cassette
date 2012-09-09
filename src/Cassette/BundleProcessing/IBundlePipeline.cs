using System;
using System.Collections.Generic;

namespace Cassette.BundleProcessing
{
    public interface IBundlePipeline<T> : IList<IBundleProcessor<T>>, IBundleProcessor<T>
        where T : Bundle
    {
        void Add<TBundleProcessor>()
            where TBundleProcessor : class, IBundleProcessor<T>;

        void Add<TBundleProcessorFactory>(Func<TBundleProcessorFactory, IBundleProcessor<T>> create)
            where TBundleProcessorFactory : class;

        void Insert<TBundleProcessor>(int index) 
            where TBundleProcessor : class, IBundleProcessor<T>;

        void Insert<TBundleProcessorFactory>(int index, Func<TBundleProcessorFactory, IBundleProcessor<T>> create) 
            where TBundleProcessorFactory : class;

        void ReplaceWith<TReplacementProcessors>()
            where TReplacementProcessors : class, IEnumerable<IBundleProcessor<T>>;
    }
}