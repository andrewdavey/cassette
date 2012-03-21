using System.Collections.Generic;
using Cassette.Configuration;

namespace Cassette.BundleProcessing
{
    public class BundlePipeline<T> : List<IBundleProcessor<T>>, IBundlePipeline<T>
        where T : Bundle
    {
        public virtual void Process(T bundle, CassetteSettings settings)
        {
            foreach (var processor in this)
            {
                processor.Process(bundle, settings);
            }
        }
    }
}