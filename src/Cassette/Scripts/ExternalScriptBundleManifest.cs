namespace Cassette.Scripts
{
    class ExternalScriptBundleManifest : ScriptBundleManifest
    {
        public string Url { get; set; }
        public string FallbackCondition { get; set; }
    }
}