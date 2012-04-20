using System;
using System.Collections.Generic;
using TinyIoC;

namespace Cassette.Stylesheets
{
    [ConfigurationOrder(10)]
    class StylesheetsContainerConfiguration : ContainerConfiguration<StylesheetBundle>
    {
        public StylesheetsContainerConfiguration(Func<Type, IEnumerable<Type>> getImplementationTypes) : base(getImplementationTypes)
        {
        }

        public override void Configure(TinyIoCContainer container)
        {
            base.Configure(container);
            container.Register(typeof(IStylesheetMinifier), typeof(MicrosoftStylesheetMinifier));
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