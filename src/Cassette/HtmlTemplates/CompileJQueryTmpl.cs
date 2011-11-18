using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class CompileJQueryTmpl : AddTransformerToAssets
    {
        public CompileJQueryTmpl()
            : base(new CompileAsset(new JQueryTmplCompiler()))
        {
        }
    }
}