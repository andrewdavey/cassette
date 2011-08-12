using System;

namespace Cassette.ModuleProcessing
{
    public class Customize<T> : IModuleProcessor<T>
        where T : Module
    {
        public Customize(Action<T> action)
        {
            if (action == null) throw new ArgumentNullException("action");
            this.action = action;
        }

        readonly Action<T> action;

        public void Process(T module, ICassetteApplication application)
        {
            action(module);
        }
    }
}
