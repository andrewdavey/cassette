using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cassette.Scripts;
using Cassette.TinyIoC;

namespace Cassette.CDN
{
    [ConfigurationOrder(10)]
    class CdnScriptContainerConfiguration : ContainerConfiguration<CdnScriptBundle>
    {
        public CdnScriptContainerConfiguration(Func<Type, IEnumerable<Type>> getImplementationTypes)
            : base(getImplementationTypes)
        {
        }

        public override void Configure(TinyIoCContainer container)
        {
            base.Configure(container);
            container.Register(typeof(IJavaScriptMinifier), typeof(MicrosoftJavaScriptMinifier));
        }

        protected override string FilePattern
        {
            get { return "*.js"; }
        }

        protected override Regex FileSearchExclude
        {
            get
            {
                return new Regex("-vsdoc.js$");
            }
        }

        protected override Type BundleFactoryType
        {
            get { return typeof(CdnScriptBundleFactory); }
        }

        protected override Type BundlePipelineType
        {
            get { return typeof(CdnScriptPipeline); }
        }
    }
}