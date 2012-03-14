using Cassette.Configuration;

namespace Cassette.Scripts.Manifests
{
    class ExternalScriptBundleManifest : ScriptBundleManifest
    {
        public string Url { get; set; }
        public string FallbackCondition { get; set; }

        protected override Bundle CreateBundleCore(CassetteSettings settings)
        {
            return new ExternalScriptBundle(Url, Path, FallbackCondition)
            {
                Renderer = new ConstantHtmlRenderer<ScriptBundle>(Html(), settings.UrlModifier)
            };
        }
    }
}