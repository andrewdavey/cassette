using Cassette.Stylesheets;

namespace Cassette.CDN
{
    public static class CdnBundleExtensions
    {
        public static void GzipScriptBundle(this CdnScriptBundle bundle, 
            bool isDebuggingEnabled, bool gzipInDebug = false)
        {
            if (isDebuggingEnabled && !gzipInDebug)
                return;

            bundle.GzipScriptBundle();
        }

        public static void GzipScriptBundle(this CdnScriptBundle bundle)
        {
            bundle.Pipeline.Insert<GzipAssetProcessor<CdnScriptBundle>>(bundle.Pipeline.Count);
        }

        public static void GzipStylesheetBundle(this CdnStylesheetBundle bundle,
            bool isDebuggingEnabled, bool gzipInDebug = false)
        {
            if (isDebuggingEnabled && !gzipInDebug)
                return;
            
            bundle.GzipStylesheetBundle();
        }

        public static void GzipStylesheetBundle(this CdnStylesheetBundle bundle)
        {
            bundle.Pipeline.Insert<GzipAssetProcessor<CdnStylesheetBundle>>(bundle.Pipeline.Count);
        }
    }
}