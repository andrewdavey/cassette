using System.IO.IsolatedStorage;
using System.Linq;

namespace Knapsack
{
    public class StylesheetModuleContainerBuilder : ModuleContainerBuilder
    {
        readonly string applicationRoot;

        public StylesheetModuleContainerBuilder(IsolatedStorageFile storage, string rootDirectory, string applicationRoot)
            : base(storage, rootDirectory)
        {
            this.applicationRoot = EnsureDirectoryEndsWithSlash(applicationRoot);
        }

        public override ModuleContainer Build()
        {
            var moduleBuilder = new UnresolvedStylesheetModuleBuilder(rootDirectory);
            var unresolvedModules = relativeModuleDirectories.Select(moduleBuilder.Build);
            var modules = UnresolvedModule.ResolveAll(unresolvedModules);
            return new ModuleContainer(
                modules, 
                storage, 
                textWriter => new StylesheetModuleWriter(textWriter, rootDirectory, applicationRoot, LoadFile)
            );
        }
    }
}
