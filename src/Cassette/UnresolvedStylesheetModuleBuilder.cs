namespace Cassette
{
    public class UnresolvedStylesheetModuleBuilder : UnresolvedModuleBuilder
    {
        readonly string applicationRoot;

        public UnresolvedStylesheetModuleBuilder(string rootDirectory, string applicationRoot)
            : base(rootDirectory, new[] { "css" })
        {
            this.applicationRoot = applicationRoot;
        }

        protected override IUnresolvedResourceParser CreateParser(string filename)
        {
            return new UnresolvedCssParser(applicationRoot);
        }
    }
}
