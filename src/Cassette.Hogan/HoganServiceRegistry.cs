using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class HoganServiceRegistry : ServiceRegistry
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