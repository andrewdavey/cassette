using System;
using System.Linq;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    class ExternalScriptBundleHtmlRenderer : IBundleHtmlRenderer<ExternalScriptBundle>
    {
        readonly IBundleHtmlRenderer<ScriptBundle> fallbackScriptRenderer;
        readonly CassetteSettings settings;

        public ExternalScriptBundleHtmlRenderer(IBundleHtmlRenderer<ScriptBundle> fallbackScriptRenderer, CassetteSettings settings)
        {
            this.fallbackScriptRenderer = fallbackScriptRenderer;
            this.settings = settings;
        }

        public string Render(ExternalScriptBundle bundle)
        {
            var externalScriptHtml = string.Format(HtmlConstants.ScriptHtml, bundle.Url);

            if (settings.IsDebuggingEnabled)
            {
                if (bundle.Assets.Any())
                {
                    return fallbackScriptRenderer.Render(bundle);                    
                }
                else
                {
                    return externalScriptHtml;
                }
            }
            else
            {
                if (bundle.Assets.Any())
                {
                    return string.Format(
                        "{1}{0}<script type=\"text/javascript\">{0}if({2}){{{0}{3}{0}}}{0}</script>",
                        Environment.NewLine,
                        externalScriptHtml,
                        bundle.FallbackCondition,
                        CreateFallbackScripts(bundle)
                    );
                }
                else
                {
                    return externalScriptHtml;                    
                }
            }
        }

        string CreateFallbackScripts(ExternalScriptBundle bundle)
        {
            var scripts = fallbackScriptRenderer.Render(bundle);
            return ConvertToDocumentWriteCalls(scripts);
        }

        string ConvertToDocumentWriteCalls(string scriptElements)
        {
            var scripts = scriptElements.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            
            return string.Join(
                Environment.NewLine,
                from script in scripts
                select "document.write(unescape('" + Escape(script) + "'));"
            );
        }

        string Escape(string script)
        {
            return script.Replace("<", "%3C").Replace(">", "%3E");
        }
    }
}

