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
        readonly string javaScriptVariableName;
        readonly AddHtmlTemplateToJavaScriptArrayTransformers transformer;

        public JavaScriptArrayHtmlTemplatePipeline(TinyIoCContainer container, CassetteSettings settings)
            : this(container, settings, "JST")
        {
        }

        public JavaScriptArrayHtmlTemplatePipeline(TinyIoCContainer container, CassetteSettings settings, string javascriptVariableName)
            : base(container)
        {
            if (string.IsNullOrEmpty(javascriptVariableName))
            {
                throw new ArgumentNullException("javascriptVariableName");
            }

            this.container = container;
            this.settings = settings;
            this.javaScriptVariableName = javascriptVariableName;
            this.transformer = container.Resolve<AddHtmlTemplateToJavaScriptArrayTransformers>();

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
            transformer.JavaScriptVariableName = javaScriptVariableName;
            Add(transformer);
        }

        void Concatenate()
        {
            Add(new ConcatenateAssets { Separator = Environment.NewLine });
        }

        void WrapJavaScriptHtmlTemplates()
        {
            Add(new WrapJavaScriptArrayHtmlTemplates(javaScriptVariableName));
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