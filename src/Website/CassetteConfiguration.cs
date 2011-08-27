using Cassette;
using Cassette.Stylesheets;

namespace Website
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(ModuleConfiguration moduleConfiguration)
        {
            moduleConfiguration.Add(new DirectorySource<StylesheetModule>("assets/styles")
            {
                FilePattern = "*.css"
            });
        }
    }
}