using System.Web;
using Cassette.Scripts;

namespace Cassette.RequireJS
{
    public class ViewHelperInitializer : IStartUpTask
    {
        readonly BundleCollection bundles;
        readonly IUrlGenerator urlGenerator;
        readonly CassetteSettings settings;

        public ViewHelperInitializer(BundleCollection bundles, IUrlGenerator urlGenerator, CassetteSettings settings)
        {
            this.bundles = bundles;
            this.urlGenerator = urlGenerator;
            this.settings = settings;
        }

        public void Start()
        {
            ViewHelper.Instance = this;
        }

        public IHtmlString RequireJsScript()
        {
            var bundle = bundles.Get<ScriptBundle>("~/Cassette.RequireJs");
            return new HtmlString(bundle.Renderer.Render(bundle));
        }
    }

    public static class ViewHelper
    {
        public static IHtmlString RequireJsScript()
        {
            return Instance.RequireJsScript();
        }

        public static ViewHelperInitializer Instance { get; set; }
    }
}
