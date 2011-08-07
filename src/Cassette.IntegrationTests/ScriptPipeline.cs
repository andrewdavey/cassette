using System;
using System.IO;
using System.Linq;
using Cassette.ModuleProcessing;
using Cassette.Persistence;
using Should;
using Xunit;

namespace Cassette.IntegrationTests
{
    public class ScriptPipeline : IDisposable
    {
        public ScriptPipeline()
        {
            cacheDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(cacheDirectory);
        }

        readonly string cacheDirectory;

        [Fact]
        public void CanBuildModuleSourceAndPipelineAndContainer()
        {
            var root = new DirectoryInfo("..\\..\\assets\\scripts").FullName;

            var source = new ModuleSource<ScriptModule>(root, "*.js")
                .AddEachSubDirectory();

            var pipeline = new Pipeline<ScriptModule>(
                new ParseJavaScriptReferences(),
                new SortAssetsByDependency(),
                new ConcatenateAssets(),
                new MinifyAssets(new MicrosoftJavaScriptMinifier())
            );

            var moduleFactory = new ScriptModuleFactory(path => Path.Combine(root, path));

            var fileSystem = new FileSystem(cacheDirectory);

            var initializer = new Initializer<ScriptModule>(
                moduleFactory, 
                new ModuleContainerReader<ScriptModule>(fileSystem, moduleFactory), 
                new ModuleContainerWriter<ScriptModule>(fileSystem)
            );
            var container = initializer.Initialize(source, pipeline);

            var result = container.ToArray();
            result[0].Assets[0].OpenStream().ReadAsString().ShouldEqual("function asset3(){}");
            result[1].Assets[0].OpenStream().ReadAsString().ShouldEqual("function asset2(){}function asset1(){}");
        }

        public void Dispose()
        {
            Directory.Delete(cacheDirectory, true);
        }
    }

    static class StreamHelpers
    {
        public static string ReadAsString(this Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
