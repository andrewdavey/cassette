using System.Reflection;
using Cassette.Scripts;

namespace Cassette.Aspnet
{
    public class DiagnosticBundleConfiguration : IConfiguration<BundleCollection>
    {
        public void Configure(BundleCollection bundles)
        {
            bundles.AddEmbeddedResources<ScriptBundle>(
                "~/Cassette.Aspnet.Resources",
                Assembly.GetExecutingAssembly(),
                "Cassette.Aspnet.Resources",

                "jquery.js",
                "knockout.js",
                "diagnostic-page.js"
            );
        }
    }
}