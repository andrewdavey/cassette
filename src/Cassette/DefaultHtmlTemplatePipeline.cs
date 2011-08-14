using System.Collections.Generic;
using Cassette.HtmlTemplates;
using Cassette.ModuleProcessing;

namespace Cassette
{
    public class DefaultHtmlTemplatePipeline : IModuleProcessor<HtmlTemplateModule>
    {
        public void Process(HtmlTemplateModule module, ICassetteApplication application)
        {
            foreach (var processor in CreatePipeline(application))
            {
                processor.Process(module, application);
            }
        }

        IEnumerable<IModuleProcessor<HtmlTemplateModule>> CreatePipeline(ICassetteApplication application)
        {
            yield return new WrapHtmlTemplatesInScriptBlocks();
            if (application.IsOutputOptimized)
            {
                yield return new ConcatenateAssets();
            }
        }
    }
}