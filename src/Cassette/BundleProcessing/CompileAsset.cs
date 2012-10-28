using Cassette.IO;

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

        public string Transform(string assetContent, IAsset asset)
        {
            var compileResult = Compile(assetContent, asset);
            AddRawFileReferenceForEachImportedFile(asset, compileResult);
            return compileResult.Output;
        }

        CompileResult Compile(string assetContent, IAsset asset)
        {
            var context = CreateCompileContext(asset);
            return compiler.Compile(assetContent, context);
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