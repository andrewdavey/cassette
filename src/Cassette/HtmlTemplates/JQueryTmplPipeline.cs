using System.Collections.Generic;
using Cassette.ModuleProcessing;

namespace Cassette.HtmlTemplates
{
    public class JQueryTmplPipeline : MutablePipeline<HtmlTemplateModule>
    {
        protected override IEnumerable<IModuleProcessor<HtmlTemplateModule>> CreatePipeline(HtmlTemplateModule module, ICassetteApplication application)
        {
            yield return new AddTransformerToAssets(new CompileAsset(new JQueryTmplCompiler()));
            yield return new ConcatenateAssets();
            yield return new AddTransformerToAssets(new WrapWithJavaScriptElement());
        }
    }
}
