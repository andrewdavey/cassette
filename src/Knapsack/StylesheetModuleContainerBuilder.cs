using System.IO.IsolatedStorage;
using System.Linq;

namespace Knapsack
{
    public class StylesheetModuleContainerBuilder : ModuleContainerBuilder
    {
        public StylesheetModuleContainerBuilder(IsolatedStorageFile storage, string rootDirectory)
            : base(storage, rootDirectory)
        {
        }

        public override ModuleContainer Build()
        {
            var moduleBuilder = new UnresolvedStylesheetModuleBuilder(rootDirectory);
            var unresolvedModules = relativeModuleDirectories.Select(moduleBuilder.Build);
            var modules = UnresolvedModule.ResolveAll(unresolvedModules);
            return new ModuleContainer(
                modules, 
                storage, 
                textWriter => new StylesheetModuleWriter(textWriter, rootDirectory, LoadFile)
            );
        }
    }
}
