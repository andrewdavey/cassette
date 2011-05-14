using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using Knapsack.CoffeeScript;

namespace Knapsack
{
    public class ScriptModuleContainerBuilder : ModuleContainerBuilder
    {
        readonly ICoffeeScriptCompiler coffeeScriptCompiler;

        public ScriptModuleContainerBuilder(IsolatedStorageFile storage, string rootDirectory, ICoffeeScriptCompiler coffeeScriptCompiler)
            : base(storage, rootDirectory)
        {
            this.coffeeScriptCompiler = coffeeScriptCompiler;
        }

        public ModuleContainer Build()
        {
            var moduleBuilder = new UnresolvedScriptModuleBuilder(rootDirectory);
            var unresolvedModules = relativeModuleDirectories.Select(moduleBuilder.Build);
            var modules = UnresolvedModule.ResolveAll(unresolvedModules);
            return new ModuleContainer(
                modules, 
                storage, 
                textWriter => new ScriptModuleWriter(textWriter, rootDirectory, LoadFile, coffeeScriptCompiler)
            );
        }
    }
}
