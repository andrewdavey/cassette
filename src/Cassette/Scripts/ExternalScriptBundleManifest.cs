namespace Cassette.Scripts
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