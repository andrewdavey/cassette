using System;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class StylesheetBundleServiceRegistry : BundleServiceRegistry<StylesheetBundle>
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