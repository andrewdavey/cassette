using System;
using System.IO;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    class DefineJavaScriptVariable : IAssetTransformer
    {
        readonly string javaScriptVariableName;

        public DefineJavaScriptVariable(string javaScriptVariableName)
        {
            this.javaScriptVariableName = javaScriptVariableName;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var source = openSourceStream().ReadToEnd();
                var define = string.Format(
                    "if (typeof {0}==='undefined'){{var {0}={{}};}}{1}",
                    javaScriptVariableName,
                    source
                );
                return define.AsStream();
            };
        }
    }
}