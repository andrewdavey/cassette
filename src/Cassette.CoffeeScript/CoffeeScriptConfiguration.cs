using Cassette.Configuration;

namespace Cassette.Scripts
{
    [InternalCassetteConfiguration]
    public class CoffeeScriptConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            settings.AddDefaultFileSearchPattern<ScriptBundle>("*.coffee");
        }
    }
}