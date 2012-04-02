using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class HoganPipeline : BundlePipeline<HtmlTemplateBundle>
    {
        public HoganPipeline(IUrlGenerator urlGenerator)
            : this("JST", urlGenerator)
        {
        }

        /// <param name="javaScriptVariableName">
        /// The name of the JavaScript variable to store compiled templates in.
        /// For example, the default is "JST", so a template will be registered as <code>JST['my-template'] = ...;</code>.
        /// </param>
        /// <param name="urlGenerator">Used to create the URLs for bundles.</param>
        public HoganPipeline(string javaScriptVariableName, IUrlGenerator urlGenerator)
        {
            AddRange(new IBundleProcessor<HtmlTemplateBundle>[]
            {
                new AssignHtmlTemplateRenderer(settings => new RemoteHtmlTemplateBundleRenderer(urlGenerator)),
                new AssignContentType("text/javascript"),
                new ParseHtmlTemplateReferences(),
                new CompileHogan(),
                new RegisterTemplatesWithHogan(javaScriptVariableName),
                new AssignHash(),
                new ConcatenateAssets()
            });
        }
    }
}