using Cassette;
using Cassette.Scripts;

namespace Jasmine
{
    public class CassetteConfiguration : IConfiguration<BundleCollection>
    {
        public void Configure(BundleCollection bundles)
        {
            // The "Scripts/app" contains the application script we'll be testing with Jasmine.
            // The "Scripts/specs" contains the Jasmine specs. It is treated just like a regular Cassette bundle.
            
            bundles.AddPerSubDirectory<ScriptBundle>("Scripts");
        }
    }
}