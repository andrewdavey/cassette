using System;
using System.IO;

namespace Cassette.ModuleProcessing
{
    public class WrapHtmlTemplateInScriptBlock : IAssetTransformer
    {
        public WrapHtmlTemplateInScriptBlock(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                throw new ArgumentException("The script block content type is required.", "contentType");
            }

            this.contentType = contentType;
        }

        readonly string contentType;

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var output = new MemoryStream();
                var outputWriter = new StreamWriter(output);

                outputWriter.Write("<script id=\"");
                outputWriter.Write(GetScriptBlockId(asset));
                outputWriter.Write("\" type=\"");
                outputWriter.Write(contentType);
                outputWriter.WriteLine("\">");
                WriteTemplateContent(openSourceStream, outputWriter);
                outputWriter.Write("</script>");
                outputWriter.Flush();

                output.Position = 0;
                return output;
            };
        }

        protected virtual string GetScriptBlockId(IAsset asset)
        {
            return Path.GetFileNameWithoutExtension(asset.SourceFilename);
        }

        void WriteTemplateContent(Func<Stream> openSourceStream, StreamWriter outputWriter)
        {
            using (var inputReader = new StreamReader(openSourceStream()))
            {
                string line = null;
                while ((line = inputReader.ReadLine()) != null)
                {
                    outputWriter.WriteLine(line);
                }
            }
        }
    }
}
