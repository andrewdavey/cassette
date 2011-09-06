using System.Collections.Generic;
using Cassette.ModuleProcessing;

namespace Cassette.HtmlTemplates
{
    public class KnockoutJQueryTmplPipeline : MutablePipeline<HtmlTemplateModule>
    {
        protected override IEnumerable<IModuleProcessor<HtmlTemplateModule>> CreatePipeline(HtmlTemplateModule module, ICassetteApplication application)
        {
            // Compile each template into JavaScript.
            yield return new AddTransformerToAssets(
                new CompileAsset(new KnockoutJQueryTmplCompiler())
            );
            yield return new Customize<HtmlTemplateModule>(
                m => m.ContentType = "text/javascript"
            );
            yield return new AddTransformerToAssets(
                new WrapJQueryTemplateInInitializer(module)
            );
            // Join all the JavaScript together
            yield return new ConcatenateAssets();
            // Assign the renderer to output a link to the module.
            yield return new AssignRenderer(
                new RemoteHtmlTemplateModuleRenderer(application.UrlGenerator)
            );
        }
    }
}