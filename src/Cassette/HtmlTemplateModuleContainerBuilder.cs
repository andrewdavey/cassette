using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;

namespace Cassette
{
    public class HtmlTemplateModuleContainerBuilder : ModuleContainerBuilder
    {
        readonly string applicationRoot;

        public HtmlTemplateModuleContainerBuilder(IsolatedStorageFile storage, string rootDirectory, string applicationRoot)
            : base(storage, rootDirectory)
        {
            this.applicationRoot = EnsureDirectoryEndsWithSlash(applicationRoot);
        }

        public override ModuleContainer Build()
        {
            var moduleBuilder = new UnresolveHtmlTemplateModuleBuilder(rootDirectory);
            var unresolvedModules = relativeModuleDirectories.Select(x => moduleBuilder.Build(x.Item1, x.Item2));
            var modules = UnresolvedModule.ResolveAll(unresolvedModules);
            return new ModuleContainer(
                modules,
                storage,
                textWriter => new HtmlTemplateModuleWriter(textWriter, rootDirectory, LoadFile)
            );
        }
    }
}
