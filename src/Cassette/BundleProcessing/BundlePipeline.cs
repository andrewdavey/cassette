using System;
using System.Collections.Generic;
using TinyIoC;

namespace Cassette.BundleProcessing
{
    public class BundlePipeline<T> : List<IBundleProcessor<T>>, IBundlePipeline<T>
        where T : Bundle
    {
        readonly TinyIoCContainer container;

        protected BundlePipeline(TinyIoCContainer container)
        {
            this.container = container;
        }

        public void Insert<TBundleProcessor>(int index)
            where TBundleProcessor : class, IBundleProcessor<T>
        {
            var step = (IBundleProcessor<T>)container.Resolve<TBundleProcessor>();
            Insert(index, step);
        }

        public void Insert<TBundleProcessorFactory>(int index, Func<TBundleProcessorFactory, IBundleProcessor<T>> create)
            where TBundleProcessorFactory : class
        {
            var step = create(container.Resolve<TBundleProcessorFactory>());
            Insert(index, step);
        }

        public virtual void Process(T bundle)
        {
            foreach (var processor in this)
            {
                processor.Process(bundle);
            }
        }
    }
}