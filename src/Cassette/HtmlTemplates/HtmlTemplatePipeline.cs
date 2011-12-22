using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplatePipeline : MutablePipeline<HtmlTemplateBundle>
    {
        protected override IEnumerable<IBundleProcessor<HtmlTemplateBundle>> CreatePipeline(HtmlTemplateBundle bundle, CassetteSettings settings)
        {
            if (bundle.IsFromCache) yield break;

            yield return new ParseHtmlTemplateReferences();
            yield return new WrapHtmlTemplatesInScriptElements();
            yield return new AssignHtmlTemplateRenderer(new InlineHtmlTemplateBundleRenderer());
            yield return new ConcatenateAssets();
        }
    }
}