using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cassette.TinyIoC;

namespace Cassette.Scripts
{
    [ConfigurationOrder(10)]
    class ScriptContainerConfiguration : ContainerConfiguration<ScriptBundle>
    {
        public ScriptContainerConfiguration(Func<Type, IEnumerable<Type>> getImplementationTypes) : base(getImplementationTypes)
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
            get { return typeof(ScriptBundleFactory); }
        }

        protected override Type BundlePipelineType
        {
            get { return typeof(ScriptPipeline); }
        }
    }
}