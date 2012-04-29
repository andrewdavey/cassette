using System;
using System.Linq;
using System.Text;
using Cassette.Utilities;

namespace Cassette.Scripts
{
    class ExternalScriptBundle : ScriptBundle, IExternalBundle, IBundleHtmlRenderer<ScriptBundle>
    {
        readonly string url;
        readonly string fallbackCondition;
        bool isDebuggingEnabled;
        IBundleHtmlRenderer<ScriptBundle> fallbackRenderer;

        public ExternalScriptBundle(string url)
            : base(url)
        {
            ValidateUrl(url);
            this.url = url;
        }

        public ExternalScriptBundle(string url, string bundlePath, string fallbackCondition = null)
            : base(bundlePath)
        {
            ValidateUrl(url);
            this.url = url;
            this.fallbackCondition = fallbackCondition;
        }

        static void ValidateUrl(string url)
        {
            if (url == null) throw new ArgumentNullException("url");
            if (url.IsNullOrWhiteSpace()) throw new ArgumentException("URL is required.", "url");
        }

        internal string FallbackCondition
        {
            get { return fallbackCondition; }
        }

        protected override void ProcessCore(CassetteSettings settings)
        {
            base.ProcessCore(settings);
            fallbackRenderer = Renderer;
            isDebuggingEnabled = settings.IsDebuggingEnabled;
            Renderer = this;
        }

        internal override bool ContainsPath(string pathToFind)
        {
            return base.ContainsPath(pathToFind) || url.Equals(pathToFind, StringComparison.OrdinalIgnoreCase);
        }

        public string ExternalUrl
        {
            get { return url; }
        }

        public string Render(ScriptBundle unusedParameter)
        {
            if (isDebuggingEnabled && Assets.Any())
            {
                return fallbackRenderer.Render(this);
            }

            var conditionalRenderer = new ConditionalRenderer();
            return conditionalRenderer.Render( Condition, html =>
            {
                if (Assets.Any())
                {
                    RenderScriptHtmlWithFallback(html);
                }
                else
                {
                    RenderScriptHtml(html);
                }
            });
        }

        void RenderScriptHtml(StringBuilder html)
        {
            html.AppendFormat(
                HtmlConstants.ScriptHtml,
                url,
                HtmlAttributes.CombinedAttributes
                );
        }

        void RenderScriptHtmlWithFallback(StringBuilder html)
        {
            html.AppendFormat(
                HtmlConstants.ScriptHtmlWithFallback,
                url,
                HtmlAttributes.CombinedAttributes,
                FallbackCondition,
                CreateFallbackScripts(),
                Environment.NewLine
                );
        }

        string CreateFallbackScripts()
        {
            var scripts = fallbackRenderer.Render(this);
            return ConvertToDocumentWriteCalls(scripts);
        }

        static string ConvertToDocumentWriteCalls(string scriptElements)
        {
            var scripts = scriptElements.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            return string.Join(
                Environment.NewLine,
                (from script in scripts
                select "document.write(unescape('" + Escape(script) + "'));").ToArray()
            );
        }

        static string Escape(string script)
        {
            return script.Replace("<", "%3C").Replace(">", "%3E");
        }
    }
}