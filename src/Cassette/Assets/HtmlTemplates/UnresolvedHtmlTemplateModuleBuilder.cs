using Cassette.ModuleBuilding;

namespace Cassette.Assets.HtmlTemplates
{
    public class UnresolvedHtmlTemplateModuleBuilder : UnresolvedModuleBuilder
    {
        public UnresolvedHtmlTemplateModuleBuilder(string rootDirectory)
            : base (rootDirectory, new[] { "htm", "html" })
        {
        }

        protected override IUnresolvedAssetParser CreateParser(string filename)
        {
            return new UnresolvedHtmlTemplateParser();
        }
    }
}
