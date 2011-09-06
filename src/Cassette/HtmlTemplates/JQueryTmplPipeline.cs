using System.Collections.Generic;
using Cassette.ModuleProcessing;

namespace Cassette.HtmlTemplates
{
    public class JQueryTmplPipeline : MutablePipeline<HtmlTemplateModule>
    {
        protected override IEnumerable<IModuleProcessor<HtmlTemplateModule>> CreatePipeline(HtmlTemplateModule module, ICassetteApplication application)
        {
            yield return new AddTransformerToAssets(
                new CompileAsset(new JQueryTmplCompiler())
            );
            yield return new Customize<HtmlTemplateModule>(
                m => m.ContentType = "text/javascript"
            );
            yield return new AddTransformerToAssets(
                new WrapJQueryTemplateInInitializer(module)
            );
            yield return new ConcatenateAssets();
            yield return new AssignRenderer(
                new RemoteHtmlTemplateModuleRenderer(application.UrlGenerator)
            );
        }
    }
}