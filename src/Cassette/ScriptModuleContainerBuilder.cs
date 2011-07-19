using System.IO.IsolatedStorage;
using System.Linq;
using Cassette.CoffeeScript;

namespace Cassette
{
    public class ScriptModuleContainerBuilder : ModuleContainerBuilder
    {
        readonly ICoffeeScriptCompiler coffeeScriptCompiler;

        public ScriptModuleContainerBuilder(IsolatedStorageFile storage, string rootDirectory, ICoffeeScriptCompiler coffeeScriptCompiler)
            : base(storage, rootDirectory)
        {
            this.coffeeScriptCompiler = coffeeScriptCompiler;
        }

        public override ModuleContainer Build()
        {
            var moduleBuilder = new UnresolvedScriptModuleBuilder(rootDirectory);
            var unresolvedModules = relativeModuleDirectories.Select(x => moduleBuilder.Build(x.Item1, x.Item2));
            var modules = UnresolvedModule.ResolveAll(unresolvedModules);
            return new ModuleContainer(
                modules, 
                storage, 
                textWriter => new ScriptModuleWriter(textWriter, rootDirectory, LoadFile, coffeeScriptCompiler)
            );
        }
    }
}
