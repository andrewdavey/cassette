using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class CompileHoganTmpl : AddTransformerToAssets
    {
        public CompileHoganTmpl()
            : base(new CompileAsset(new HoganTmplCompiler()))
        {
        }
    }
}
