using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cassette.ModuleProcessing
{
    public class HtmlTemplatesInScriptBlocks : IModuleProcessor<HtmlTemplateModule>
    {
        private string p;

        public HtmlTemplatesInScriptBlocks(string p)
        {
            // TODO: Complete member initialization
            this.p = p;
        }

        public void Process(HtmlTemplateModule module)
        {
            throw new NotImplementedException();
        }
    }
}
