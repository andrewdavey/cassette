using System;

namespace Cassette
{
    public class HtmlTemplateModule : Module
    {
        public HtmlTemplateModule(string directory, IFileSystem fileSystem)
            : base(directory, fileSystem)
        {
        }
    }
}