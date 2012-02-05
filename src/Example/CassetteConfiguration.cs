﻿using Cassette.Configuration;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Example
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            bundles.AddPerSubDirectory<ScriptBundle>("Scripts");
            bundles.AddUrlWithAlias(
                "http://platform.twitter.com/widgets.js",
                "twitter",
                b => { b.PageLocation = "body"; b.HtmlAttributes.Add(new { async = "async" }); });
            
            bundles.AddPerSubDirectory<HtmlTemplateBundle>(
                "HtmlTemplates",
                bundle => bundle.Processor = new HoganPipeline()
                {
                    Namespace = "templates"
                }
            );

            bundles.Add<StylesheetBundle>("Styles", b => b.Processor = new StylesheetPipeline()
                .EmbedImages(whitelistPath => whitelistPath.Contains("/embed/"))
                .EmbedFonts(whitelistPath => whitelistPath.Contains("/embed/")));
        }
    }
}