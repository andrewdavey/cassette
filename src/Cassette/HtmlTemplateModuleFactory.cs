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

        public HtmlTemplateModule CreateExternalModule(string url)
        {
            throw new NotSupportedException("External HTML template modules are not supported.");
        }
    }
}