using System.Linq;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.Web.Jasmine
{
    public class JasmineBundleConfiguration : IConfiguration<BundleCollection>
    {
        readonly IBundleFactory<ScriptBundle> scriptBundleFactory;
        readonly IBundleFactory<StylesheetBundle> stylesheetBundleFactory;

        public JasmineBundleConfiguration(IBundleFactory<ScriptBundle> scriptBundleFactory, IBundleFactory<StylesheetBundle> stylesheetBundleFactory)
        {
            this.scriptBundleFactory = scriptBundleFactory;
            this.stylesheetBundleFactory = stylesheetBundleFactory;
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
            var script = scriptBundleFactory.CreateBundle("cassette.web.jasmine", Enumerable.Empty<IFile>(), new BundleDescriptor());
            script.Assets.Add(new ResourceAsset("Cassette.Web.Jasmine.jasmine.js", GetType().Assembly));
            return script;
        }

        StylesheetBundle CreateStylesheetBundle()
        {
            var css = stylesheetBundleFactory.CreateBundle("cassette.web.jasmine", Enumerable.Empty<IFile>(), new BundleDescriptor());
            css.Assets.Add(new ResourceAsset("Cassette.Web.Jasmine.jasmine.css", GetType().Assembly));
            return css;
        }
    }
}