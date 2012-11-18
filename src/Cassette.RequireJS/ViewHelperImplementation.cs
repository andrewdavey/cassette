using System.Collections.Generic;
using System.Web;
using Cassette.Scripts;
#if NET35
using Cassette.Views;
#endif

namespace Cassette.RequireJS
{
    public class ViewHelperImplementation
    {
        readonly BundleCollection bundles;
        readonly IAmdConfiguration configuration;
        readonly IJsonSerializer jsonSerializer;
        readonly IRequireJsConfigUrlProvider requireJsConfigUrlProvider;

        public ViewHelperImplementation(BundleCollection bundles, IAmdConfiguration configuration, IJsonSerializer jsonSerializer, IRequireJsConfigUrlProvider requireJsConfigUrlProvider)
        {
            this.bundles = bundles;
            this.configuration = configuration;
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
            var bundle = bundles.Get<ScriptBundle>(configuration.MainBundlePath);
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