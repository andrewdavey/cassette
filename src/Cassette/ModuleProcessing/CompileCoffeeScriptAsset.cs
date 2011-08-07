using System;
using System.IO;
using Cassette.CoffeeScript;

namespace Cassette.ModuleProcessing
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
                writer.Flush();
                // Do not close writer, because this will also close the outputStream,
                // which need to the caller return.

                outputStream.Position = 0;
                return outputStream;
            };
        }
    }
}