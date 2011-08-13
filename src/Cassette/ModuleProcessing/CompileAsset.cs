using System;
using System.IO;
using Cassette.Utilities;

namespace Cassette.ModuleProcessing
{
    public class CompileAsset : IAssetTransformer
    {
        public CompileAsset(ICompiler compiler)
        {
            this.compiler = compiler;
        }

        readonly ICompiler compiler;

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                using (var input = new StreamReader(openSourceStream()))
                {
                    var css = compiler.Compile(input.ReadToEnd(), asset.SourceFilename, asset.Directory);
                    return css.AsStream();
                }
            };
        }
    }
}