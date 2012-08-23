using Cassette;
﻿using Cassette.Configuration;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Website
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.IsHtmlRewritingEnabled = false;
            settings.UrlModifier = new StaticDomainUrlModifier(settings.UrlModifier);
            bundles.Add<StylesheetBundle>("assets/styles");
            bundles.Add<StylesheetBundle>("assets/iestyles", b => b.Condition = "IE");

            bundles.AddPerSubDirectory<ScriptBundle>("assets/scripts");
            bundles.AddUrlWithLocalAssets(
                "//ajax.googleapis.com/ajax/libs/jquery/1.6.3/jquery.min.js",
                new LocalAssetSettings
                {
                    FallbackCondition = "!window.jQuery",
                    Path =  "assets/scripts/jquery"
                }
            );
        }
    }

    public class StaticDomainUrlModifier : IUrlModifier
    {
        private readonly IUrlModifier baseModifier;

        public StaticDomainUrlModifier(IUrlModifier baseModifier)
        {
            this.baseModifier = baseModifier;
        }

        /// <summary>
        /// Exists just to satisfy interface requirements. Just calls modify of another IUrlModifier.
        /// </summary>
        public string PreCacheModify(string url)
        {
            return baseModifier.PreCacheModify(url);
        }

        /// <summary>
        /// Prepends the static domain to the beginning of the server relative URL path.
        /// </summary>
        public string PostCacheModify(string url)
        {
            return "//" + "www.google.com" + url;
        }
    }
}