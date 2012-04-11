using System;
using System.Collections.Generic;

namespace Cassette.HtmlTemplates
{
    class HtmlTemplateBundleContainerBuilder : BundleContainerModule<HtmlTemplateBundle>
    {
        public HtmlTemplateBundleContainerBuilder(Func<Type, IEnumerable<Type>> getImplementationTypes) : base(getImplementationTypes)
        {
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