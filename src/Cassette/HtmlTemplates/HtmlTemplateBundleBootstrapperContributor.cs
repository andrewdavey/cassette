using System;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateBundleBootstrapperContributor : BundleBootstrapperContributor<HtmlTemplateBundle>
    {
        protected override Type BundlePipeline
        {
            get { return typeof(HtmlTemplatePipeline); }
        }

        protected override Type BundleFactory
        {
            get { return typeof(HtmlTemplateBundleFactory); }
        }
    }
}