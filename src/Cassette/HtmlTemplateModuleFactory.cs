using System;

namespace Cassette
{
    public class HtmlTemplateModuleFactory : IModuleFactory<HtmlTemplateModule>
    {
        public HtmlTemplateModuleFactory(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        readonly IFileSystem fileSystem;

        public HtmlTemplateModule CreateModule(string directory)
        {
            return new HtmlTemplateModule(directory, fileSystem);
        }
    }
}