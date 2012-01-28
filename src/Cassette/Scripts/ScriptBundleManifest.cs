namespace Cassette.Scripts
{
    class ScriptBundleManifest : BundleManifest
    {
        protected override Bundle CreateBundleCore()
        {
            return new ScriptBundle(Path);
        }
    }
}