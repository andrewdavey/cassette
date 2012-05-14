using System;
using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class ParseHtmlTemplateReferences : ParseReferences<HtmlTemplateBundle>
    {
        protected override bool ShouldParseAsset(IAsset asset)
        {
            return asset.Path.EndsWith(".htm", StringComparison.OrdinalIgnoreCase)
                || asset.Path.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                || asset.Path.EndsWith(".jst", StringComparison.OrdinalIgnoreCase)
                || asset.Path.EndsWith(".tmpl", StringComparison.OrdinalIgnoreCase)
                || asset.Path.EndsWith(".mustache", StringComparison.OrdinalIgnoreCase);
        }
        
        internal override ICommentParser CreateCommentParser()
        {
            return new HtmlTemplateCommentParser();
        }
    }
}
