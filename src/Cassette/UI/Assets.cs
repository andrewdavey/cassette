using System;
using System.Collections.Generic;
using System.Web;

namespace Cassette.UI
{
    // Partial backwards compatibility with Cassette 0.8.
    // Allow people to keep using the Assets helper in their views.

    [Obsolete("The Assets helper class is obsolete. Use the Bundles helper class instead.")]
    public static class Assets
    {
        public static ObsoleteBundleHelper Scripts
        {
            get { return new ObsoleteScriptHelper(); }
        }

        public static ObsoleteBundleHelper Stylesheets
        {
            get { return new ObsoleteBundleHelper(Bundles.RenderStylesheets); }
        }

        public static ObsoleteBundleHelper HtmlTemplates
        {
            get { return new ObsoleteBundleHelper(Bundles.RenderHtmlTemplates); }
        }
    }

    [Obsolete("This class will be removed from the next version of Cassette. Do not use.")]
    public class ObsoleteBundleHelper
    {
        readonly Func<string, IHtmlString> render;

        internal ObsoleteBundleHelper(Func<string, IHtmlString> render)
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

        public string ModuleUrl(string path)
        {
            return Bundles.Url(path);
        }
    }

    [Obsolete]
    public class ObsoleteScriptHelper : ObsoleteBundleHelper
    {
        public ObsoleteScriptHelper()
            : base(Bundles.RenderScripts)
        {
        }

        public void AddInline(string scriptContent, string pageLocation = null)
        {
            Bundles.AddInlineScript(scriptContent, pageLocation);
        }

        public void AddPageData(string globalVariable, object data, string pageLocation = null)
        {
            Bundles.AddPageData(globalVariable, data, pageLocation);
        }

// ReSharper disable ParameterTypeCanBeEnumerable.Global
        public void AddPageData(string globalVariable, IDictionary<string, object> data, string pageLocation = null)
// ReSharper restore ParameterTypeCanBeEnumerable.Global
        {
            Bundles.AddPageData(globalVariable, data, pageLocation);
        }
    }
}