using System;

namespace Cassette
{
    public class HtmlTemplateModule : Module
    {
        public HtmlTemplateModule(string directory, IFileSystem fileSystem)
            : base(directory, fileSystem)
        {
        }

        public override string ContentType
        {
            get { return "text/html"; }
        }
    }
}