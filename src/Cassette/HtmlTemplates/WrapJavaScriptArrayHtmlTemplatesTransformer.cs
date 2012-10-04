using System;
using System.IO;
using System.Text;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    public class WrapJavaScriptArrayHtmlTemplatesTransformer : IAssetTransformer
    {
        readonly string contentType;

        public WrapJavaScriptArrayHtmlTemplatesTransformer(string contentType)
        {
            this.contentType = contentType;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return () =>
            {
                var addTemplateCalls = openSourceStream().ReadToEnd();

                var output = string.Format(
                    @"(function(d) {{
var addTemplate = function(id, content) {{
    var script = d.createElement('script');
    script.type = '{0}';
    script.id = id;
    if (typeof script.textContent !== 'undefined') {{
        script.textContent = content;
    }} else {{
        script.innerText = content;
    }}
    var x = d.getElementsByTagName('script')[0];
    x.parentNode.insertBefore(script, x);
}};
{1}
}}(document));",
                    contentType,
                    addTemplateCalls);

                return new MemoryStream(Encoding.UTF8.GetBytes(output));
            };
        }
    }
}