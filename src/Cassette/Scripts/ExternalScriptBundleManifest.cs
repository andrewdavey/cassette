namespace Cassette.Scripts
{
    class ExternalScriptBundleManifest : BundleManifest
    {
        public string Url { get; set; }
        public string FallbackCondition { get; set; }
    }
}