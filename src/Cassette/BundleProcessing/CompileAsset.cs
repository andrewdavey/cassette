using System;
using System.IO;
using Cassette.Utilities;
using Cassette.IO;

namespace Cassette.BundleProcessing
{
    class CompileAsset : IAssetTransformer
    {
        public CompileAsset(ICompiler compiler, IDirectory rootDirectory)
        {
            this.compiler = compiler;
            this.rootDirectory = rootDirectory;
        }

        readonly ICompiler compiler;
        readonly IDirectory rootDirectory;

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                using (var input = new StreamReader(openSourceStream()))
                {
                    var context = new CompileContext
                    {
                        SourceFilePath = asset.Path,
                        RootDirectory = rootDirectory
                    };
                    var compileResult = compiler.Compile(input.ReadToEnd(), context);
                    // TODO: for each compileResource.ImportedFilePaths -> asset.RawFileReference
                    return compileResult.Output.AsStream();
                }
            };
        }
    }
}