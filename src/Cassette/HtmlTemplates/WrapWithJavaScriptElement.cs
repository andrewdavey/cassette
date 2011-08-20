using System;
using System.IO;

namespace Cassette.HtmlTemplates
{
    public class WrapWithJavaScriptElement : IAssetTransformer
    {
        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var output = new MemoryStream();
                var writer = new StreamWriter(output);
                writer.WriteLine("<script type=\"text/javascript\">");
                writer.Flush();
                using (var template = openSourceStream())
                {
                    template.CopyTo(output);
                }
                writer.WriteLine();
                writer.WriteLine("</script>");
                writer.Flush();
                output.Position = 0;
                return output;
            };
        }
    }
}