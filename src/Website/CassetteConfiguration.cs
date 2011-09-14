using Cassette;
using Cassette.Stylesheets;

namespace Website
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(ModuleConfiguration moduleConfiguration, ICassetteApplication application)
        {
            moduleConfiguration.Add(new DirectorySource<StylesheetModule>("assets/styles")
            {
                FilePattern = "*.css"
            });
        }
    }
}
