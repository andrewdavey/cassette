using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cassette.ModuleProcessing
{
    public class ConditionalStep<T> : IModuleProcessor<T>
        where T : Module
    {
        public ConditionalStep(
            Func<Module, ICassetteApplication, bool> condition, 
            params IModuleProcessor<T>[] children)
        {
            this.condition = condition;
            this.children = children;
        }

        readonly Func<Module, ICassetteApplication, bool> condition;
        readonly IModuleProcessor<T>[] children;

        public void Process(T module, ICassetteApplication application)
        {
            if (condition(module, application) == false) return;

            ProcessEachChild(module, application);
        }

        void ProcessEachChild(T module, ICassetteApplication application)
        {
            foreach (var child in children)
            {
                child.Process(module, application);
            }
        }
    }
}
