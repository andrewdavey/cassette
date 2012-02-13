using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class HoganPipeline : MutablePipeline<HtmlTemplateBundle>
    {
        /// <summary>
        /// The name of the JavaScript variable to store compiled templates in.
        /// For example, the default is "JST", so a template will be registered as <code>JST['my-template'] = ...;</code>.
        /// </summary>
        public string JavaScriptVariableName { get; set; }
        
        protected override IEnumerable<IBundleProcessor<HtmlTemplateBundle>> CreatePipeline(HtmlTemplateBundle bundle, CassetteSettings settings)
        {
            yield return new AssignHtmlTemplateRenderer(
                new RemoteHtmlTemplateBundleRenderer(settings.UrlGenerator)
            );
            yield return new AssignContentType("text/javascript");
            if (bundle.IsFromCache) yield break;

            yield return new ParseHtmlTemplateReferences();
            yield return new CompileHogan();
            yield return new RegisterTemplatesWithHogan(bundle, JavaScriptVariableName);
            yield return new AssignHash();
            yield return new ConcatenateAssets();
        }
    }
}