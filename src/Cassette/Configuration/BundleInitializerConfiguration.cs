using System.IO;
using System.Text.RegularExpressions;

namespace Cassette.Configuration
{
    public class BundleInitializerConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.DefaultAssetSources[typeof(Scripts.ScriptBundle)] = CreateScriptBundleInitializer();
            settings.DefaultAssetSources[typeof(Stylesheets.StylesheetBundle)] = CreateStylesheetBundleInitializer();
            settings.DefaultAssetSources[typeof(HtmlTemplates.HtmlTemplateBundle)] = CreateHtmlTemplateBundleInitializer();
        }

        AssetSource CreateScriptBundleInitializer()
        {
            return new AssetSource
            {
                FilePattern = "*.js;*.coffee",
                ExcludeFilePath = new Regex("-vsdoc\\.js"),
                SearchOption = SearchOption.AllDirectories
            };
        }

        AssetSource CreateStylesheetBundleInitializer()
        {
            return new AssetSource
            {
                FilePattern = "*.css;*.less",
                SearchOption = SearchOption.AllDirectories
            };
        }

        AssetSource CreateHtmlTemplateBundleInitializer()
        {
            return new AssetSource
            {
                FilePattern = "*.htm;*.html",
                SearchOption = SearchOption.AllDirectories
            };
        }
    }
}