using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.Stylesheets;
using Cassette.TinyIoC;

namespace Cassette.CDN
{
    public class CdnExpandCssUrls : IBundleProcessor<CdnStylesheetBundle>
    {
        readonly CassetteSettings settings;
        readonly TinyIoCContainer container;

        public CdnExpandCssUrls (TinyIoCContainer container)
        {
            this.container = container;
            this.settings = container.Resolve<CassetteSettings>();
        }

        public void Process(CdnStylesheetBundle bundle)
        {
            var parameters = new NamedParameterOverloads(new Dictionary<string, object> { { "cdnRoot", bundle.CdnRoot } });
            var cdnUrlGenerator = container.Resolve<CdnUrlGenerator>(parameters);
            
            foreach (var asset in bundle.Assets)
            {                
                asset.AddAssetTransformer(new ExpandCssUrlsAssetTransformer(settings.SourceDirectory, cdnUrlGenerator));
            }
        }
    }
}