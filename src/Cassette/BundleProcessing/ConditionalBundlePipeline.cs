using System;
using Cassette.Configuration;

namespace Cassette.BundleProcessing
{
    public class ConditionalBundlePipeline<T> : BundlePipeline<T>
        where T : Bundle
    {
        readonly Func<CassetteSettings, bool> shouldProcess;
        
        public ConditionalBundlePipeline(Func<CassetteSettings, bool> shouldProcess, params IBundleProcessor<T>[] processors)
        {
            this.shouldProcess = shouldProcess;
            AddRange(processors);
        }

        public override void Process(T bundle, CassetteSettings settings)
        {
            if (!shouldProcess(settings)) return;
            base.Process(bundle, settings);
        }
    }
}