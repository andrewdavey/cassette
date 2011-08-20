using System.Collections.Generic;
using Cassette.ModuleProcessing;

namespace Cassette.HtmlTemplates
{
    public class JQueryTmplPipeline : MutablePipeline<HtmlTemplateModule>
    {
        public bool KnockoutJS { get; set; }

        protected override IEnumerable<IModuleProcessor<HtmlTemplateModule>> CreatePipeline(HtmlTemplateModule module, ICassetteApplication application)
        {
            var compiler = KnockoutJS ? new KnockoutJQueryTmplCompiler() : new JQueryTmplCompiler();
            yield return new AddTransformerToAssets(new CompileAsset(compiler));
            yield return new ConcatenateAssets();
            yield return new AddTransformerToAssets(new WrapWithJavaScriptElement());
        }
    }
}
