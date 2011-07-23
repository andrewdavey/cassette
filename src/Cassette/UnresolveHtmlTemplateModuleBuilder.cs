namespace Cassette
{
    public class UnresolveHtmlTemplateModuleBuilder : UnresolvedModuleBuilder
    {
        public UnresolveHtmlTemplateModuleBuilder(string rootDirectory)
            : base (rootDirectory, new[] { "htm", "html" })
        {
        }

        protected override IUnresolvedAssetParser CreateParser(string filename)
        {
            return new UnresolvedHtmlTemplateParser();
        }
    }
}
