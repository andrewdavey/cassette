using System.Collections.Generic;
using TinyIoC;

namespace Cassette.BundleProcessing
{
    public class BundlePipeline<T> : List<IBundleProcessor<T>>, IBundlePipeline<T>
        where T : Bundle
    {
        public TinyIoCContainer Container { get; private set; }

        protected BundlePipeline(TinyIoCContainer container)
        {
            Container = container;
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