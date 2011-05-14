using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;

namespace Knapsack
{
    public class StylesheetModuleContainerBuilder : ModuleContainerBuilder
    {
        public StylesheetModuleContainerBuilder(IsolatedStorageFile storage, string rootDirectory)
            : base(storage, rootDirectory)
        {
        }

        public ModuleContainer Build()
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
