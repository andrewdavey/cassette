using Cassette.Manifests;

namespace Cassette.Scripts.Manifests
{
    class ScriptBundleManifest : BundleManifest
    {
        public string Condition { get; set; }

        protected override Bundle CreateBundleCore(IUrlModifier urlModifier)
        {
            return new ScriptBundle(Path)
            {
                Condition = Condition,
                Renderer = new ConstantHtmlRenderer<ScriptBundle>(Html(), urlModifier)
            };
        }
    }
}