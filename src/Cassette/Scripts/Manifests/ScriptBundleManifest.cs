using Cassette.Manifests;

namespace Cassette.Scripts.Manifests
{
    class ScriptBundleManifest : BundleManifest
    {
        protected override Bundle CreateBundleCore()
        {
            return new ScriptBundle(Path);
        }
    }
}