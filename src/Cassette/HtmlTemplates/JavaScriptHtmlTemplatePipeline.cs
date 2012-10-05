using System;
using Cassette.BundleProcessing;
using Cassette.Scripts;
using Cassette.TinyIoC;

namespace Cassette.HtmlTemplates
{
    public class JavaScriptHtmlTemplatePipeline : BundlePipeline<HtmlTemplateBundle>
    {
        public delegate JavaScriptHtmlTemplatePipeline Factory(IHtmlTemplateScriptStrategy scriptStrategy);

        readonly CassetteSettings settings;
        readonly IJavaScriptMinifier minifier;
        readonly IBundleHtmlRenderer<HtmlTemplateBundle> renderer;

        public JavaScriptHtmlTemplatePipeline(TinyIoCContainer container, CassetteSettings settings, IJavaScriptMinifier minifier, IBundleHtmlRenderer<HtmlTemplateBundle> renderer)
            : base(container)
        {
            this.settings = settings;
            this.minifier = minifier;
            this.renderer = renderer;
            BuildPipeline();
        }

        void BuildPipeline()
        {
            TransformHtmlTemplatesIntoJavaScript();
            Concatenate();
            WrapJavaScriptHtmlTemplates();
            MinifyIfNotDebugging();
            AssignContentType();
            AssignHash();
            AssignRenderer();
        }

        void TransformHtmlTemplatesIntoJavaScript()
        {
            Add<AddHtmlTemplateToJavaScriptTransformers>();
        }

        void Concatenate()
        {
            Add(new ConcatenateAssets { Separator = Environment.NewLine });
        }

        void WrapJavaScriptHtmlTemplates()
        {
            Add<WrapJavaScriptHtmlTemplates>();
        }

        void MinifyIfNotDebugging()
        {
            if (!settings.IsDebuggingEnabled)
            {
                Add(new MinifyAssets(minifier));
            }
        }

        void AssignContentType()
        {
            Add(new AssignContentType("text/javascript"));
        }

        void AssignHash()
        {
            Add(new AssignHash());
        }

        void AssignRenderer()
        {
            Add(new AssignHtmlTemplateRenderer(renderer));
        }
    }
}