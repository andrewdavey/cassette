﻿using Cassette.Configuration;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Backbone
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            bundles.Add<StylesheetBundle>(
                "styles/todos",
                (bundle) => bundle.Processor = new StylesheetPipeline()
                    .EmbedImages()
                    //.EmbedFonts()
            );
            bundles.Add<ScriptBundle>(
                "scripts/lib"
            );
            bundles.Add<ScriptBundle>(
                "scripts/todos"
            );
            bundles.Add<HtmlTemplateBundle>(
                "scripts/todos/templates",
                (bundle) => bundle.Processor = new HoganPipeline(){
                    Namespace = "JST"
                }
            );
        }
    }
}