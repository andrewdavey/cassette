using System;
using System.Collections.Generic;
using Cassette.Scripts;
using Cassette.TinyIoC;

namespace Cassette.HtmlTemplates
{
    [ConfigurationOrder(10)]
    class HtmlTemplatesContainerConfiguration : ContainerConfiguration<HtmlTemplateBundle>
    {
        public HtmlTemplatesContainerConfiguration(Func<Type, IEnumerable<Type>> getImplementationTypes) : base(getImplementationTypes)
        {
        }

        public override void Configure(TinyIoCContainer container)
        {
            base.Configure(container);
            container.Register(
                (c, n) => new JavaScriptHtmlTemplatePipeline(
                    c,
                    c.Resolve<CassetteSettings>(),
                    c.Resolve<IJavaScriptMinifier>(),
                    c.Resolve<RemoteHtmlTemplateBundleRenderer>()
                )
            );
            container.Register<IHtmlTemplateScriptStrategy, DomHtmlTemplateScriptStrategy>();
            // For compatibility with previous version of Cassette,
            // pathSeparatorReplacement is "-" by default
            container.Register<IHtmlTemplateIdStrategy>((c, n) => new HtmlTemplateIdBuilder(pathSeparatorReplacement: "-"));
        }

        protected override string FilePattern
        {
            get { return "*.htm;*.html"; }
        }

        protected override Type BundleFactoryType
        {
            get { return typeof(HtmlTemplateBundleFactory); }
        }

        protected override Type BundlePipelineType
        {
            get { return typeof(HtmlTemplatePipeline); }
        }
    }
}