namespace Cassette.Scripts.Manifests
{
    class ExternalScriptBundleManifest : ScriptBundleManifest
    {
        public string Url { get; set; }
        public string FallbackCondition { get; set; }

        protected override Bundle CreateBundleCore()
        {
            return new ExternalScriptBundle(Url, Path, FallbackCondition);
        }
    }
}