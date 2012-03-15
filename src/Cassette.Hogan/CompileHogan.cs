using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class CompileHogan : AddTransformerToAssets
    {
        public CompileHogan()
            : base(new CompileAsset(new HoganCompiler()))
        {
        }
    }
}
