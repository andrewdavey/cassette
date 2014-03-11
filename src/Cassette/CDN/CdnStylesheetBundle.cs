using System;
using Cassette.BundleProcessing;
using Cassette.IO;
using Cassette.Stylesheets;

namespace Cassette.CDN
{
    public sealed class CdnStylesheetBundle : ExternalStylesheetBundle, ICdnBundle
    {                
        public CdnStylesheetBundle(string url) : base(url)
        {
        }

        public CdnStylesheetBundle(string url, string applicationRelativePath) : base(url, applicationRelativePath)
        {
        }

        public string CdnRoot { get; set; }
        public string CdnCacheRoot { get; set; }

        public override string ExternalUrl
        {
            get
            {
                return !String.IsNullOrEmpty(CdnRoot) 
                    ? !String.IsNullOrEmpty(CdnCacheRoot)
                        ? String.Join("/", new[] { CdnRoot.TrimEnd('/'), CdnCacheRoot, CacheFilename })
                        : String.Join("/", new[] { CdnRoot.TrimEnd('/'), CacheFilename }) 
                    : base.ExternalUrl;
            }
        }

        public new IBundlePipeline<CdnStylesheetBundle> Pipeline { get; set; }

        protected override void ProcessCore(CassetteSettings settings)
        {
            Pipeline.Process(this);
            FallbackRenderer = Renderer;
            Renderer = new ExternalStylesheetBundleRenderer(settings);            
        }
    }
}
