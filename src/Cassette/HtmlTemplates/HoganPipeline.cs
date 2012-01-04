using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class HoganPipeline : MutablePipeline<HtmlTemplateBundle>
    {
        protected override IEnumerable<IBundleProcessor<HtmlTemplateBundle>> CreatePipeline(HtmlTemplateBundle bundle, CassetteSettings settings)
        {
            yield return new AssignHtmlTemplateRenderer(
                new RemoteHtmlTemplateBundleRenderer(settings.UrlGenerator)
            );
            yield return new AssignContentType("text/javascript");
            if (bundle.IsFromCache) yield break;

            yield return new ParseHtmlTemplateReferences();
            yield return new CompileHogan();
            yield return new RegisterTemplatesWithHogan(bundle);
            yield return new ConcatenateAssets();
        }
    }
}