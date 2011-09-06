using System;
using System.IO;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    public class WrapJQueryTemplateInInitializer : IAssetTransformer
    {
        readonly HtmlTemplateModule module;

        public WrapJQueryTemplateInInitializer(HtmlTemplateModule module)
        {
            this.module = module;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var id = module.GetTemplateId(asset);
                var template = openSourceStream().ReadToEnd();
                return string.Format("$.template('{0}', {1});{2}", id, template, Environment.NewLine).AsStream();
            };
        }
    }
}