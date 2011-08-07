using System;

namespace Cassette
{
    public class HtmlTemplateModule : Module
    {
        public HtmlTemplateModule(string directory, Func<string, string> getFullPath)
            : base(directory, getFullPath)
        {
        }
    }
}