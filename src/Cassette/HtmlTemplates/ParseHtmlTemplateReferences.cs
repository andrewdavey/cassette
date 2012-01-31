﻿using System;
using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class ParseHtmlTemplateReferences : ParseReferences<HtmlTemplateBundle>
    {
        protected override bool ShouldParseAsset(IAsset asset)
        {
            return asset.SourceFile.FullPath.EndsWith(".htm", StringComparison.OrdinalIgnoreCase)
                || asset.SourceFile.FullPath.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                || asset.SourceFile.FullPath.EndsWith(".jst", StringComparison.OrdinalIgnoreCase)
                || asset.SourceFile.FullPath.EndsWith(".tmpl", StringComparison.OrdinalIgnoreCase)
                || asset.SourceFile.FullPath.EndsWith(".mustache", StringComparison.OrdinalIgnoreCase);
        }
        
        internal override ICommentParser CreateCommentParser()
        {
            return new HtmlTemplateCommentParser();
        }
    }
}
