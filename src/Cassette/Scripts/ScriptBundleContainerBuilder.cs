using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cassette.Scripts
{
    class ScriptBundleContainerBuilder : BundleContainerModule<ScriptBundle>
    {
        public ScriptBundleContainerBuilder(Func<Type, IEnumerable<Type>> getImplementationTypes) : base(getImplementationTypes)
        {
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