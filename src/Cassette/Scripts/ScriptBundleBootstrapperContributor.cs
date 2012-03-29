using System;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    public class ScriptBundleBootstrapperContributor : BundleBootstrapperContributor<ScriptBundle>
    {
        protected override Type BundlePipeline
        {
            // TODO: Rename ScriptPipeline to ScriptBundlePipeline? Same for the other bundle types...
            get { return typeof(ScriptPipeline); }
        }

        protected override Type BundleFactory
        {
            get { return typeof(ScriptBundleFactory); }
        }
    }
}