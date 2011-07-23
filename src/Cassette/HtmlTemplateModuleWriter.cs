using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cassette
{
    public class HtmlTemplateModuleWriter : IModuleWriter
    {
        readonly TextWriter textWriter;
        readonly string rootDirectory;
        readonly Func<string, string> readFileText;

        public HtmlTemplateModuleWriter(TextWriter textWriter, string rootDirectory, Func<string, string> readFileText)
        {
            this.textWriter = textWriter;
            this.rootDirectory = rootDirectory;
            this.readFileText = readFileText;
        }

        public void Write(Module module)
        {
            var scriptTemplate = "<script id=\"{0}\" type=\"text/html\">{1}</script>";
            
            var scripts = from asset in module.Assets
                          let id = Path.GetFileNameWithoutExtension(asset.Path)
                          let content = readFileText(rootDirectory + asset.Path)
                          select string.Format(scriptTemplate, id, content);

            var allHtml = string.Join("\r\n", scripts);
            textWriter.Write(allHtml);
        }
    }
}
