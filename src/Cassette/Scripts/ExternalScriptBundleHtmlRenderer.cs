using System;
using System.Linq;
using System.Text;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    class ExternalScriptBundleHtmlRenderer : IBundleHtmlRenderer<ScriptBundle>
    {
        readonly IBundleHtmlRenderer<ScriptBundle> fallbackScriptRenderer;
        readonly CassetteSettings settings;

        public ExternalScriptBundleHtmlRenderer(IBundleHtmlRenderer<ScriptBundle> fallbackScriptRenderer, CassetteSettings settings)
        {
            this.fallbackScriptRenderer = fallbackScriptRenderer;
            this.settings = settings;
        }

        public string Render(ScriptBundle bundle)
        {
            if (settings.IsDebuggingEnabled && bundle.Assets.Any())
            {
                return fallbackScriptRenderer.Render(bundle);
            }

            var html = new StringBuilder();
            
            if (bundle.Assets.Any())
            {
                html.AppendFormat(
                    HtmlConstants.ScriptHtmlWithFallback,
                    bundle.Url,
                    bundle.HtmlAttributes.CombinedAttributes,
                    ((ExternalScriptBundle)bundle).FallbackCondition,
                    CreateFallbackScripts(bundle),
                    Environment.NewLine
                );
            }
            else
            {
                html.AppendFormat(
                    HtmlConstants.ScriptHtml,
                    bundle.Url,
                    bundle.HtmlAttributes.CombinedAttributes
                );
            }

            if (bundle.HasCondition)
            {
                return bundle.ConditionalRenderer.RenderCondition(bundle.Condition, html.ToString());
            }
            else
            {
                return html.ToString();
            }
        }

        string CreateFallbackScripts(ScriptBundle bundle)
        {
            var scripts = fallbackScriptRenderer.Render(bundle);
            return ConvertToDocumentWriteCalls(scripts);
        }

        static string ConvertToDocumentWriteCalls(string scriptElements)
        {
            var scripts = scriptElements.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            
            return string.Join(
                Environment.NewLine,
                from script in scripts
                select "document.write(unescape('" + Escape(script) + "'));"
            );
        }

        static string Escape(string script)
        {
            return script.Replace("<", "%3C").Replace(">", "%3E");
        }
    }
}