using System;
using Cassette.BundleProcessing;
using Cassette.IO;
using Cassette.Scripts;

namespace Cassette.CDN
{
    public sealed class CdnScriptBundle : ExternalScriptBundle, ICdnBundle
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

        public new IBundlePipeline<CdnScriptBundle> Pipeline { get; set; }

        protected override void ProcessCore(CassetteSettings settings)
        {
            Pipeline.Process(this);
            FallbackRenderer = Renderer;
            Renderer = new ExternalScriptBundleRenderer(settings);
        }
    }
}
