using System;
using Cassette;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Spriting;

namespace Website
{
    public class CassetteConfiguration : IConfiguration<BundleCollection>
    {
        public void Configure(BundleCollection bundles)
        {
            bundles.Add<StylesheetBundle>("assets/styles", b => b.SpriteImages());
            bundles.Add<StylesheetBundle>("assets/iestyles", b => b.Condition = "IE");
            
            bundles.AddPerSubDirectory<ScriptBundle>("assets/scripts");
            bundles.AddUrlWithLocalAssets(
                "//ajax.googleapis.com/ajax/libs/jquery/1.6.3/jquery.min.js",
                new LocalAssetSettings
                {
                    FallbackCondition = "!window.jQuery",
                    Path = "assets/scripts/jquery"
                }
            );
        }
    }

    public class FileAccessAuthorizationConfiguration : IConfiguration<IFileAccessAuthorization>
    {
        public void Configure(IFileAccessAuthorization files)
        {
            files.AllowAccess(path => path.StartsWith("~/assets/styles/images", StringComparison.OrdinalIgnoreCase));
        }
    }
}