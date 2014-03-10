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
            var parameters = new NamedParameterOverloads(new Dictionary<string, object> { { "bundle", bundle } });
            var cdnUrlGenerator = container.Resolve<CdnUrlGenerator<CdnStylesheetBundle>>(parameters);
            
            foreach (var asset in bundle.Assets)
            {                
                asset.AddAssetTransformer(new ExpandCssUrlsAssetTransformer(settings.SourceDirectory, cdnUrlGenerator));
            }
        }
    }
}