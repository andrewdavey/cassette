using System.IO.IsolatedStorage;
using System.Linq;
using Cassette.ModuleBuilding;

namespace Cassette.Assets.HtmlTemplates
{
    public class HtmlTemplateModuleContainerBuilder : ModuleContainerBuilder
    {
        public HtmlTemplateModuleContainerBuilder(IsolatedStorageFile storage, string rootDirectory, string applicationRoot)
            : base(storage, rootDirectory)
        {
        }

        public override ModuleContainer Build()
        {
            var moduleBuilder = new UnresolvedHtmlTemplateModuleBuilder(rootDirectory);
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
