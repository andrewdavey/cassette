using System;

namespace Cassette.HtmlTemplates
{
    class HtmlTemplateBundleContainerModule : BundleContainerModule<HtmlTemplateBundle>
    {
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