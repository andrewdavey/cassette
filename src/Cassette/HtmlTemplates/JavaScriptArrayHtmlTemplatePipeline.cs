using System;
using Cassette.BundleProcessing;
using Cassette.Scripts;
using Cassette.TinyIoC;

namespace Cassette.HtmlTemplates
{
    public class JavaScriptArrayHtmlTemplatePipeline : BundlePipeline<HtmlTemplateBundle>
    {
        readonly TinyIoCContainer container;
        readonly CassetteSettings settings;

        public JavaScriptArrayHtmlTemplatePipeline(TinyIoCContainer container, CassetteSettings settings)
            : base(container)
        {
            this.container = container;
            this.settings = settings;
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
            Add<AddHtmlTemplateToJavaScriptArrayTransformers>();
        }

        void Concatenate()
        {
            Add(new ConcatenateAssets { Separator = Environment.NewLine });
        }

        void WrapJavaScriptHtmlTemplates()
        {
            Add(new WrapJavaScriptHtmlTemplates());
        }

        void MinifyIfNotDebugging()
        {
            if (!settings.IsDebuggingEnabled)
            {
                Add(new MinifyAssets(container.Resolve<IJavaScriptMinifier>()));
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
            Add(new AssignHtmlTemplateRenderer(container.Resolve<RemoteHtmlTemplateBundleRenderer>()));
        }
    }
}