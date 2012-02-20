using Cassette.Configuration;
using Cassette.Scripts;

namespace Jasmine
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            // The "Scripts/app" contains the application script we'll be testing with Jasmine.
            // The "Scripts/specs" contains the Jasmine specs. It is treated just like a regular Cassette bundle.
            // The "Scripts/jasmine" is usually provided by the Cassette.Web.Jasmine nuget package.

            bundles.Add<ScriptBundle>("Scripts/app");
            bundles.Add<ScriptBundle>("Scripts/specs");
        }
    }
}