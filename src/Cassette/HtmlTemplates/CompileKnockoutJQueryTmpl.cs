using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class CompileKnockoutJQueryTmpl : AddTransformerToAssets
    {
        public CompileKnockoutJQueryTmpl()
            : base(new CompileAsset(new KnockoutJQueryTmplCompiler()))
        {
        }
    }
}
