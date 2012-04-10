using System;

namespace Cassette.Stylesheets
{
    class StylesheetBundleContainerModule : BundleContainerModule<StylesheetBundle>
    {
        protected override string FilePattern
        {
            get { return "*.css"; }
        }

        protected override Type BundleFactoryType
        {
            get { return typeof(StylesheetBundleFactory); }
        }

        protected override Type BundlePipelineType
        {
            get { return typeof(StylesheetPipeline); }
        }
    }
}