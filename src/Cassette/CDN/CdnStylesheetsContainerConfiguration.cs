using System;
using System.Collections.Generic;
using Cassette.Stylesheets;
using Cassette.TinyIoC;

namespace Cassette.CDN
{
    [ConfigurationOrder(10)]
    class CdnStylesheetsContainerConfiguration : ContainerConfiguration<StylesheetBundle>
    {
        public CdnStylesheetsContainerConfiguration(Func<Type, IEnumerable<Type>> getImplementationTypes)
            : base(getImplementationTypes)
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
            get { return typeof(CdnStylesheetBundleFactory); }
        }

        protected override Type BundlePipelineType
        {
            get { return typeof(StylesheetPipeline); }
        }
    }
}