using System;
using System.IO;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.BundleProcessing
{
    public class CompileAsset : IAssetTransformer
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
                    var compileResult = Compile(asset, input);
                    AddRawFileReferenceForEachImportedFile(asset, compileResult);
                    return compileResult.Output.AsStream();
                }
            };
        }

        CompileResult Compile(IAsset asset, StreamReader input)
        {
            var context = CreateCompileContext(asset);
            return compiler.Compile(input.ReadToEnd(), context);
        }

        CompileContext CreateCompileContext(IAsset asset)
        {
            return new CompileContext
            {
                SourceFilePath = asset.Path,
                RootDirectory = rootDirectory
            };
        }

        void AddRawFileReferenceForEachImportedFile(IAsset asset, CompileResult compileResult)
        {
            foreach (var importedFilePath in compileResult.ImportedFilePaths)
            {
                asset.AddRawFileReference(importedFilePath);
            }
        }
    }
}