using System;
using Cassette.Scripts;

namespace Cassette.CDN
{
    public class CdnScriptBundle : ExternalScriptBundle, ICdnBundle
    {
        public CdnScriptBundle(string url)
            : base(url)
        {
        }

        public CdnScriptBundle(string url, string bundlePath, string fallbackCondition = null)
            : base(url, bundlePath, fallbackCondition)
        {
        }

        public string CdnRoot { get; set; }

        public override string ExternalUrl
        {
            get
            {
                return !String.IsNullOrEmpty(CdnRoot) 
                    ? String.Join("/", new[] { CdnRoot.TrimEnd('/'), CacheFilename }) 
                    : base.ExternalUrl;
            }
        }
    }
}
