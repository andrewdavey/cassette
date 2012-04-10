using System;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateBundleServiceRegistry : BundleServiceRegistry<HtmlTemplateBundle>
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