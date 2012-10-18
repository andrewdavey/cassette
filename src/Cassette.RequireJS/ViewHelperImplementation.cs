using System.Collections.Generic;
using System.Web;
using Cassette.Scripts;

namespace Cassette.RequireJS
{
    public class ViewHelperImplementation
    {
        readonly BundleCollection bundles;
        readonly RequireJsSettings settings;
        readonly IJsonSerializer jsonSerializer;
        readonly RequireJsConfigUrlProvider requireJsConfigUrlProvider;

        public ViewHelperImplementation(BundleCollection bundles, RequireJsSettings settings, IJsonSerializer jsonSerializer, RequireJsConfigUrlProvider requireJsConfigUrlProvider)
        {
            this.bundles = bundles;
            this.settings = settings;
            this.jsonSerializer = jsonSerializer;
            this.requireJsConfigUrlProvider = requireJsConfigUrlProvider;
        }

        public IHtmlString RequireJsScript(params string[] initialModules)
        {
            using (bundles.GetReadLock())
            {
                return new HtmlString(
                    ConfigScriptElement() + 
                    MainScriptElements() + 
                    InitScriptElement(initialModules)
                );
            }
        }

        string ConfigScriptElement()
        {
            return "<script type=\"text/javascript\" src=\"" + requireJsConfigUrlProvider.Url + "\"></script>";
        }

        string MainScriptElements()
        {
            var bundle = bundles.Get<ScriptBundle>(settings.MainBundlePath);
            return bundle.Renderer.Render(bundle);
        }

        string InitScriptElement(IEnumerable<string> initialModules)
        {
            var paths = jsonSerializer.Serialize(initialModules);
            return InlineScriptElement("require(" + paths + ")");
        }

        string InlineScriptElement(string script)
        {
            return "<script type=\"text/javascript\">" + script + "</script>";
        }
    }
}