namespace Knapsack
{
    public class UnresolvedStylesheetModuleBuilder : UnresolvedModuleBuilder
    {
        public UnresolvedStylesheetModuleBuilder(string rootDirectory)
            : base(rootDirectory, new[] { "css" })
        {
        }

        protected override IUnresolvedResourceParser CreateParser(string filename)
        {
            return new UnresolvedCssParser();
        }
    }
}
