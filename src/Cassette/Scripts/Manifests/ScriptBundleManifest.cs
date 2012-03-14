using Cassette.Configuration;
using Cassette.Manifests;

namespace Cassette.Scripts.Manifests
{
    class ScriptBundleManifest : BundleManifest
    {
        public string Condition { get; set; }

        protected override Bundle CreateBundleCore(CassetteSettings settings)
        {
            return new ScriptBundle(Path)
            {
                Condition = Condition,
                Renderer = new ConstantHtmlRenderer<ScriptBundle>(Html(), settings.UrlModifier)
            };
        }
    }
}