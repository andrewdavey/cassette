using System;
using System.Collections.Generic;

namespace Cassette.HtmlTemplates
{
    [ConfigurationOrder(10)]
    class HtmlTemplatesContainerConfiguration : ContainerConfiguration<HtmlTemplateBundle>
    {
        public HtmlTemplatesContainerConfiguration(Func<Type, IEnumerable<Type>> getImplementationTypes) : base(getImplementationTypes)
        {
        }

        public override void Configure(TinyIoC.TinyIoCContainer container)
        {
            base.Configure(container);
            container.Register<IHtmlTemplateIdStrategy>(
                // For compatibility with previous version of Cassette,
                // pathSeparatorReplacement is "-" by default
                (c, n) => new HtmlTemplateIdBuilder(pathSeparatorReplacement: "-")
            );
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