using System;
using System.Collections.Generic;

namespace Cassette.Stylesheets
{
    class StylesheetContainerConfiguration : ContainerConfiguration<StylesheetBundle>
    {
        public StylesheetContainerConfiguration(Func<Type, IEnumerable<Type>> getImplementationTypes) : base(getImplementationTypes)
        {
        }

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