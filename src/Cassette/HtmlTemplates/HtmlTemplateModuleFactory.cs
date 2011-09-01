using System;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateModuleFactory : IModuleFactory<HtmlTemplateModule>
    {
        public HtmlTemplateModule CreateModule(string directory)
        {
            return new HtmlTemplateModule(directory);
        }

        public HtmlTemplateModule CreateExternalModule(string url)
        {
            throw new NotSupportedException("External HTML template modules are not supported.");
        }

        public HtmlTemplateModule CreateExternalModule(string name, ModuleDescriptor moduleDescriptor)
        {
            throw new NotSupportedException("External HTML template modules are not supported.");
        }
    }
}