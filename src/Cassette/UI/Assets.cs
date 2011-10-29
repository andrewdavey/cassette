using System;
using System.Web;

namespace Cassette.UI
{
    // Partial backwards compatibility with Cassette 0.8.
    // Allow people to keep using the Assets helper in their views.

    [Obsolete("The Assets helper class is obsolete. Use the Bundles helper class instead.")]
    public static class Assets
    {
        public static BundleType Scripts
        {
            get { return new BundleType(Bundles.RenderScripts); }
        }

        public static BundleType Stylesheets
        {
            get { return new BundleType(Bundles.RenderStylesheets); }
        }

        public static BundleType HtmlTemplates
        {
            get { return new BundleType(Bundles.RenderHtmlTemplates); }
        }
    }

    [Obsolete("This class will be removed from the next version of Cassette. Do not use.")]
    public sealed class BundleType
    {
        readonly Func<string, IHtmlString> render;

        internal BundleType(Func<string, IHtmlString> render)
        {
            this.render = render;
        }

        public void Reference(string path, string pageLocation = null)
        {
            Bundles.Reference(path, pageLocation);
        }

        public IHtmlString Render(string pageLocation = null)
        {
            return render(pageLocation);
        }
    }
}