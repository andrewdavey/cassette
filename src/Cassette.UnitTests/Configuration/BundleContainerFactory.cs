using System.Collections.Generic;
using Xunit;

namespace Cassette.Configuration
{
    public class BundleContainerFactory_Tests : BundleContainerFactoryTestSuite
    {
        internal override IBundleContainerFactory CreateFactory(IEnumerable<Bundle> bundles)
        {
            return new BundleContainerFactory(new BundleCollection(Settings, bundles), Settings);
        }

        [Fact]
        public void GivenHtmlTemplateBundleWithNoAssets_DontCrashOnRender()
        {
            var bundle = new HtmlTemplates.HtmlTemplateBundle("testPath");
            Settings.IsDebuggingEnabled = true;
            var containerFactory = CreateFactory(new[] { bundle });
            var container = containerFactory.CreateBundleContainer();
            bundle.Render();
        }
    }
}