using System.IO;
using System.Text.RegularExpressions;
using Cassette.Scripts;

namespace Cassette.Configuration
{
    [InternalCassetteConfiguration]
    public class ScriptBundleConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.ModifyDefaults<ScriptBundle>(defaults =>
            {
                defaults.FileSearch = new FileSearch
                {
                    Pattern = "*.js",
                    SearchOption = SearchOption.AllDirectories,
                    Exclude = new Regex("-vsdoc\\.js")
                };

                defaults.BundleFactory = new ScriptBundleFactory(settings);

                defaults.BundlePipeline = new ScriptPipeline();
            });
        }
    }
}