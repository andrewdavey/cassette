using System.IO;
using System.Text.RegularExpressions;

namespace Cassette.Configuration
{
    public class BundleInitializerConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.DefaultBundleInitializers[typeof(Scripts.ScriptBundle)] = CreateScriptBundleInitializer();
            settings.DefaultBundleInitializers[typeof(Stylesheets.StylesheetBundle)] = CreateStylesheetBundleInitializer();
            settings.DefaultBundleInitializers[typeof(HtmlTemplates.HtmlTemplateBundle)] = CreateHtmlTemplateBundleInitializer();
        }

        BundleDirectoryInitializer CreateScriptBundleInitializer()
        {
            return new BundleDirectoryInitializer
            {
                FilePattern = "*.js;*.coffee",
                ExcludeFilePath = new Regex("-vsdoc\\.js"),
                SearchOption = SearchOption.AllDirectories
            };
        }

        BundleDirectoryInitializer CreateStylesheetBundleInitializer()
        {
            return new BundleDirectoryInitializer
            {
                FilePattern = "*.css;*.less",
                SearchOption = SearchOption.AllDirectories
            };
        }

        BundleDirectoryInitializer CreateHtmlTemplateBundleInitializer()
        {
            return new BundleDirectoryInitializer
            {
                FilePattern = "*.htm;*.html",
                SearchOption = SearchOption.AllDirectories
            };
        }
    }
}