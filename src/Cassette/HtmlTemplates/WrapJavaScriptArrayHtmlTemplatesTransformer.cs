using System;
using System.IO;
using System.Text;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    public class WrapJavaScriptArrayHtmlTemplatesTransformer : IAssetTransformer
    {
        readonly string javascriptVariableName;

        public WrapJavaScriptArrayHtmlTemplatesTransformer(string javascriptVariableName)
        {
            if (string.IsNullOrEmpty(javascriptVariableName))
            {
                javascriptVariableName = "JST";
            }

            this.javascriptVariableName = javascriptVariableName;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return () =>
            {
                
                var addTemplateCalls = openSourceStream().ReadToEnd();

                var output = string.Format(@"window.{0}=window.{0} || {{}};
{1}", javascriptVariableName, addTemplateCalls);

                return new MemoryStream(Encoding.UTF8.GetBytes(output));
            };
        }
    }
}