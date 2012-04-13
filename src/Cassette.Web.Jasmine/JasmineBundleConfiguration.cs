using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.Web.Jasmine
{
    public class JasmineBundleConfiguration : IConfiguration<BundleCollection>
    {
        readonly IJavaScriptMinifier javaScriptMinifier;
        readonly IStylesheetMinifier stylesheetMinifier;
        readonly IUrlGenerator urlGenerator;

        public JasmineBundleConfiguration(IJavaScriptMinifier javaScriptMinifier, IStylesheetMinifier stylesheetMinifier, IUrlGenerator urlGenerator)
        {
            this.javaScriptMinifier = javaScriptMinifier;
            this.stylesheetMinifier = stylesheetMinifier;
            this.urlGenerator = urlGenerator;
        }

        public void Configure(BundleCollection bundles)
        {
            var script = CreateScriptBundle();
            bundles.Add(script);

            var css = CreateStylesheetBundle();
            bundles.Add(css);
        }

        ScriptBundle CreateScriptBundle()
        {
            var script = new ScriptBundle("cassette.web.jasmine")
            {
                Processor = new ScriptPipeline(javaScriptMinifier, urlGenerator)
            };
            script.Assets.Add(new ResourceAsset("Cassette.Web.Jasmine.jasmine.js", GetType().Assembly));
            return script;
        }

        StylesheetBundle CreateStylesheetBundle()
        {
            var css = new StylesheetBundle("cassette.web.jasmine")
            {
                Processor = new StylesheetPipeline(stylesheetMinifier, urlGenerator)
            };
            css.Assets.Add(new ResourceAsset("Cassette.Web.Jasmine.jasmine.css", GetType().Assembly));
            return css;
        }
    }
}