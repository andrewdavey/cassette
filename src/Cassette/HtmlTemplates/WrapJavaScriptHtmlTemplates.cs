using System;
using System.IO;
using System.Text;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    public class WrapJavaScriptHtmlTemplates : IAssetTransformer
    {
        readonly string contentType;

        public WrapJavaScriptHtmlTemplates(string contentType)
        {
            this.contentType = contentType;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return () =>
            {
                var start = @"(function(d) {
var addTemplate = function(id,content) {
    var script = d.createElement('script');
    script.setAttribute('type','" + contentType + @"');
    script.setAttribute('id',id);
    d.body.appendChild(script);
};
";
                var javaScript = openSourceStream().ReadToEnd();
                var end = @"
}(document));";

                var output = start + javaScript + end;
                return new MemoryStream(Encoding.UTF8.GetBytes(output));
            };
        }
    }
}