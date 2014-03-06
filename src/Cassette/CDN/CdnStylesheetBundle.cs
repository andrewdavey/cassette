using System;
using Cassette.Stylesheets;

namespace Cassette.CDN
{
    public class CdnStylesheetBundle : ExternalStylesheetBundle, ICdnBundle
    {                
        public CdnStylesheetBundle(string url) : base(url)
        {
        }

        public CdnStylesheetBundle(string url, string applicationRelativePath) : base(url, applicationRelativePath)
        {
        }

        public string CdnRoot { get; set; }

        public override string ExternalUrl
        {
            get
            {
                return !String.IsNullOrEmpty(CdnRoot) ?
                    String.Join("/", new[] { CdnRoot.TrimEnd('/'), CacheFilename }) :
                    base.ExternalUrl;
            }
        }
    }
}
