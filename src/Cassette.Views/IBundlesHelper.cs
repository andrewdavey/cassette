using System.Collections.Generic;
using System.Web;

namespace Cassette.Views
{
    public interface IBundlesHelper
    {
        void Reference(string assetPathOrBundlePathOrUrl, string pageLocation);

        void Reference<T>(string assetPathOrBundlePathOrUrl, string pageLocation)
            where T: Bundle;

        void Reference(Bundle bundle, string pageLocation);
        
        IHtmlString Render<T>(string location) where T : Bundle;

        IEnumerable<Bundle> GetReferencedBundles(string pageLocation);

        IEnumerable<string> GetReferencedBundleUrls<T>(string pageLocation)
            where T : Bundle;

        string Url<T>(string bundlePath)
            where T : Bundle;

        string FileUrl(string applicationRelativeFilePath);

        void RebuildBundleCache();
    }
}