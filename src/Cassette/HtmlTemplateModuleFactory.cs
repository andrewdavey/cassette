using System;

namespace Cassette
{
    public class HtmlTemplateModuleFactory : IModuleFactory<HtmlTemplateModule>
    {
        public HtmlTemplateModuleFactory(Func<string, string> getFullPath)
        {
            this.getFullPath = getFullPath;
        }

        readonly Func<string, string> getFullPath;

        public HtmlTemplateModule CreateModule(string directory)
        {
            return new HtmlTemplateModule(directory, getFullPath);
        }
    }
}