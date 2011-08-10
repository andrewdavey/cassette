using System;
using System.IO;
using Cassette.Utilities;

namespace Cassette.ModuleProcessing
{
    public class WrapHtmlTemplateInScriptBlock : IAssetTransformer
    {
        public WrapHtmlTemplateInScriptBlock(HtmlTemplateModule module)
        {
            this.module = module;
        }

        readonly HtmlTemplateModule module;

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                return module.RenderTemplate(openSourceStream, asset).AsStream();
            };
        }
    }
}
