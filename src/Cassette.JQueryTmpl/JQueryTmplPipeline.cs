using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class JQueryTmplPipeline : MutablePipeline<HtmlTemplateBundle>
    {
        protected override IEnumerable<IBundleProcessor<HtmlTemplateBundle>> CreatePipeline(HtmlTemplateBundle bundle, CassetteSettings settings)
        {
            yield return new AssignHtmlTemplateRenderer(
                new RemoteHtmlTemplateBundleRenderer(settings.UrlGenerator)
            );
            yield return new AssignContentType("text/javascript");
            yield return new ParseHtmlTemplateReferences();
            yield return new CompileJQueryTmpl();
            yield return new RegisterTemplatesWithJQueryTmpl(bundle);
            yield return new AssignHash();
            if (!settings.IsDebuggingEnabled)
            {
                yield return new ConcatenateAssets();
            }
        }
    }
}