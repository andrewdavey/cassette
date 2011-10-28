using System.IO;
using System.Text.RegularExpressions;

namespace Cassette.Configuration
{
    public class BundleInitializerConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.DefaultFileSources[typeof(Scripts.ScriptBundle)] = CreateScriptBundleInitializer();
            settings.DefaultFileSources[typeof(Stylesheets.StylesheetBundle)] = CreateStylesheetBundleInitializer();
            settings.DefaultFileSources[typeof(HtmlTemplates.HtmlTemplateBundle)] = CreateHtmlTemplateBundleInitializer();
        }

        FileSource CreateScriptBundleInitializer()
        {
            return new FileSource
            {
                FilePattern = "*.js;*.coffee",
                ExcludeFilePath = new Regex("-vsdoc\\.js"),
                SearchOption = SearchOption.AllDirectories
            };
        }

        FileSource CreateStylesheetBundleInitializer()
        {
            return new FileSource
            {
                FilePattern = "*.css;*.less",
                SearchOption = SearchOption.AllDirectories
            };
        }

        FileSource CreateHtmlTemplateBundleInitializer()
        {
            return new FileSource
            {
                FilePattern = "*.htm;*.html",
                SearchOption = SearchOption.AllDirectories
            };
        }
    }
}