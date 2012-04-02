using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class HoganBootstrapperContributor : BootstrapperContributor
    {
        public override IEnumerable<TypeRegistration> TypeRegistrations
        {
            get
            {
                // Replace the default bundle pipeline for HtmlTemplateBundles.
                yield return new TypeRegistration(typeof(IBundlePipeline<HtmlTemplateBundle>), typeof(HoganPipeline));
            }
        }
    }
}