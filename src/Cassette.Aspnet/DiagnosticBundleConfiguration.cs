using System.Linq;
using System.Reflection;
using Cassette.IO;
using Cassette.Scripts;

namespace Cassette.Aspnet
{
    public class DiagnosticBundleConfiguration : IConfiguration<BundleCollection>
    {
        readonly IBundleFactory<ScriptBundle> scriptBundleFactory;

        public DiagnosticBundleConfiguration(IBundleFactory<ScriptBundle> scriptBundleFactory)
        {
            this.scriptBundleFactory = scriptBundleFactory;
        }

        public void Configure(BundleCollection bundles)
        {
            var scriptBundle = scriptBundleFactory.CreateBundle(
                "~/Cassette.Aspnet.Resources",
                Enumerable.Empty<IFile>(),
                new BundleDescriptor { AssetFilenames = { "*" } }
                );
            scriptBundle.Assets.Add(new ResourceAsset("Cassette.Aspnet.Resources.jquery.js", Assembly.GetExecutingAssembly()));
            scriptBundle.Assets.Add(new ResourceAsset("Cassette.Aspnet.Resources.knockout.js", Assembly.GetExecutingAssembly()));
            scriptBundle.Assets.Add(new ResourceAsset("Cassette.Aspnet.Resources.diagnostic-page.js", Assembly.GetExecutingAssembly()));
            bundles.Add(scriptBundle);
        }
    }
}