using System;
using System.Linq;

namespace Cassette.ModuleProcessing
{
    public class Pipeline<T> : IModuleProcessor<T>
        where T : Module
    {
        public Pipeline(params IModuleProcessor<T>[] steps)
        {
            if (steps == null)
            {
                throw new ArgumentNullException("steps");
            }
            if (steps.Any(step => step == null))
            {
                throw new ArgumentException("Pipeline steps cannot be null.");
            }

            this.steps = steps;
        }

        readonly IModuleProcessor<T>[] steps;

        public void Process(T module)
        {
            foreach (var step in steps)
            {
                step.Process(module);
            }
        }
    }
}
