using System;
using System.Text.RegularExpressions;

namespace Cassette.Scripts
{
    class ScriptBundleContainerModule : BundleContainerModule<ScriptBundle>
    {
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