﻿using Cassette;
﻿using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Backbone
{
    public class CassetteConfiguration : IConfiguration<BundleCollection>
    {
        public void Configure(BundleCollection bundles)
        {
            bundles.Add<StylesheetBundle>(
                "styles/todos",
                bundle => bundle.EmbedImages().EmbedFonts()
            );
            bundles.Add<ScriptBundle>(
                "scripts/lib"
            );
            bundles.Add<ScriptBundle>(
                "scripts/todos"
            );
            bundles.Add<HtmlTemplateBundle>(
                "scripts/todos/templates"
            );
        }
    }
}