using System;
using System.Linq;
using System.Text;
using Cassette.Configuration;
using Cassette.Manifests;
using Cassette.Scripts.Manifests;

namespace Cassette.Scripts
{
    class ExternalScriptBundle : ScriptBundle, IExternalBundle, IBundleHtmlRenderer<ScriptBundle>
    {
        readonly string url;
        readonly string fallbackCondition;
        bool isDebuggingEnabled;

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
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL is required.", "url");
        }

        internal string FallbackCondition
        {
            get { return fallbackCondition; }
        }

        internal IBundleHtmlRenderer<ScriptBundle> FallbackRenderer { get; set; } 

        protected override void ProcessCore(CassetteSettings settings)
        {
            base.ProcessCore(settings);
            FallbackRenderer = Renderer;
            isDebuggingEnabled = settings.IsDebuggingEnabled;
            Renderer = this;
        }

        internal override bool ContainsPath(string pathToFind)
        {
            return base.ContainsPath(pathToFind) || url.Equals(pathToFind, StringComparison.OrdinalIgnoreCase);
        }

        internal override BundleManifest CreateBundleManifest(bool includeProcessedBundleContent)
        {
            var builder = new ExternalScriptBundleManifestBuilder { IncludeContent = includeProcessedBundleContent };
            return builder.BuildManifest(this);
        }

        public string ExternalUrl
        {
            get { return url; }
        }

        public string Render(ScriptBundle unusedParameter)
        {
            if (isDebuggingEnabled && Assets.Any())
            {
                return FallbackRenderer.Render(this);
            }

            var html = new StringBuilder();

            var hasCondition = !string.IsNullOrEmpty(Condition);
            RenderConditionalCommentStart(html, hasCondition);
            if (Assets.Any())
            {
                RenderScriptHtmlWithFallback(html);
            }
            else
            {
                RenderScriptHtml(html);
            }
            RenderConditionalCommentEnd(html, hasCondition);

            return html.ToString();
        }

        void RenderConditionalCommentStart(StringBuilder html, bool hasCondition)
        {
            if (!hasCondition) return;

            html.AppendFormat(HtmlConstants.ConditionalCommentStart, Condition);
            html.AppendLine();
        }

        void RenderConditionalCommentEnd(StringBuilder html, bool hasCondition)
        {
            if (!hasCondition) return;

            html.AppendLine();
            html.Append(HtmlConstants.ConditionalCommentEnd);
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
            var scripts = FallbackRenderer.Render(this);
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