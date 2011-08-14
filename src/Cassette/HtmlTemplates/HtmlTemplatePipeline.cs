using System.Collections.Generic;
using Cassette.ModuleProcessing;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplatePipeline : IModuleProcessor<HtmlTemplateModule>
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