using System;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class StylesheetBundleBootstrapperContributor : BundleBootstrapperContributor<StylesheetBundle>
    {
        protected override Type BundlePipeline
        {
            get { return typeof(StylesheetPipeline); }
        }

        protected override Type BundleFactory
        {
            get { return typeof(StylesheetBundleFactory); }
        }
    }
}