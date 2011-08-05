using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cassette.CoffeeScript;

namespace Cassette
{
    public class CompileCoffeeScriptAsset : IAssetTransformer
    {
        public CompileCoffeeScriptAsset(ICoffeeScriptCompiler compiler)
        {
            this.compiler = compiler;
        }

        readonly ICoffeeScriptCompiler compiler;

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                string source;
                using (var sourceStream = new StreamReader(openSourceStream()))
                {
                    source = sourceStream.ReadToEnd();
                }

                var javaScript = compiler.Compile(source, asset.SourceFilename);
                var outputStream = new MemoryStream();
                var writer = new StreamWriter(outputStream);
                writer.Write(javaScript);
                return outputStream;
            };
        }
    }
}
