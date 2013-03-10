using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Cassette.Utilities;

namespace Cassette.Scripts
{
#pragma warning disable 659
    public class ExternalScriptBundle : ScriptBundle, IExternalBundle
    {
        readonly string url;
        readonly string fallbackCondition;

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
            FallbackRenderer = Renderer;
            Renderer = new ExternalScriptBundleRenderer(settings);
        }

        internal override bool ContainsPath(string pathToFind)
        {
            return base.ContainsPath(pathToFind) || url.Equals(pathToFind, StringComparison.OrdinalIgnoreCase);
        }

        public override IEnumerable<string> GetUrls(bool isDebuggingEnabled, IUrlGenerator urlGenerator)
        {
            if (isDebuggingEnabled && Assets.Any())
            {
                return base.GetUrls(true, urlGenerator);
            }
            else
            {
                return new[] { ExternalUrl };
            }
        }

        public string ExternalUrl
        {
            get { return url; }
        }

        public IBundleHtmlRenderer<ScriptBundle> FallbackRenderer { get; set; }

        internal override void SerializeInto(XContainer container)
        {
            var serializer = new ExternalScriptBundleSerializer(container);
            serializer.Serialize(this);
        }

        public override bool Equals(object obj)
        {
            var other = obj as ExternalScriptBundle;
            return base.Equals(obj)
                   && other != null
                   && other.url == url;
        }

        public class ExternalScriptBundleRenderer : IBundleHtmlRenderer<ScriptBundle>
        {
            readonly CassetteSettings settings;

            public ExternalScriptBundleRenderer(CassetteSettings settings)
            {
                this.settings = settings;
            }

            public string Render(ScriptBundle bundle)
            {
                return Render((ExternalScriptBundle)bundle);
            }

            public string Render(ExternalScriptBundle bundle)
            {
                if (settings.IsDebuggingEnabled && bundle.Assets.Any())
                {
                    return bundle.FallbackRenderer.Render(bundle);
                }

                var conditionalRenderer = new ConditionalRenderer();
                return conditionalRenderer.Render(bundle.Condition, html =>
                {
                    if (bundle.Assets.Any())
                    {
                        RenderScriptHtmlWithFallback(html, bundle);
                    }
                    else
                    {
                        RenderScriptHtml(html, bundle);
                    }
                });
            }

            void RenderScriptHtml(StringBuilder html, ExternalScriptBundle bundle)
            {
                html.AppendFormat(
                    HtmlConstants.ScriptHtml,
                    bundle.url,
                    bundle.HtmlAttributes.CombinedAttributes
                );
            }

            void RenderScriptHtmlWithFallback(StringBuilder html, ExternalScriptBundle bundle)
            {
                html.AppendFormat(
                    HtmlConstants.ScriptHtmlWithFallback,
                    bundle.url,
                    bundle.HtmlAttributes.CombinedAttributes,
                    bundle.FallbackCondition,
                    CreateFallbackScripts(bundle),
                    Environment.NewLine
                );
            }

            string CreateFallbackScripts(ExternalScriptBundle bundle)
            {
                var scripts = bundle.FallbackRenderer.Render(bundle);
                return ConvertToDocumentWriteCalls(scripts);
            }

            static string ConvertToDocumentWriteCalls(string scriptElements)
            {
                var scripts = scriptElements.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                return string.Join(
                    Environment.NewLine,
                    (from script in scripts
                     select "document.write('" + Escape(script) + "');").ToArray()
                );
            }

            static string Escape(string script)
            {
                return script.Replace("</script>", "<\\/script>").Replace("'", @"\'");
            }
        }
    }
#pragma warning restore 659

    
}