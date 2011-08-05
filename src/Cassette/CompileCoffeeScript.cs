using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cassette
{
    public class CompileCoffeeScript : IModuleProcessor<Module>
    {
        public CompileCoffeeScript(CoffeeScript.ICoffeeScriptCompiler coffeeScriptCompiler)
        {
            this.coffeeScriptCompiler = coffeeScriptCompiler;
        }

        readonly CoffeeScript.ICoffeeScriptCompiler coffeeScriptCompiler;

        public void Process(Module module)
        {
            throw new NotImplementedException();
        }
    }
}
