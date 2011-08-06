using System;
using System.IO;
using System.Linq;
using Xunit;
using Should;

namespace Cassette.IntegrationTests
{
    public class ScriptPipeline
    {
        [Fact]
        public void CanBuildModuleSourceAndPipelineAndContainer()
        {
            var root = new DirectoryInfo("..\\..\\assets\\scripts").FullName;

            var source = new ModuleSource<ScriptModule>(root, "*.js")
                .AddEachSubDirectory();

            var pipeline = new Pipeline<ScriptModule>(
                new ParseJavaScriptReferences(),
                new SortAssetsByDependency<ScriptModule>(),
                new ConcatentateAssets(),
                new MinifyAssets(new MicrosoftJavaScriptMinifier())
            );

            var moduleFactory = new ScriptModuleFactory();
            Func<string, Stream> openFile = _ => null;
            var initializer = new Initializer<ScriptModule>(moduleFactory, new ModuleContainerStore<ScriptModule>(openFile, moduleFactory));
            var container = initializer.Initialize(source, pipeline);

            var result = container.ToArray();
            result[0].Assets[0].OpenStream().ReadAsString().ShouldEqual("function asset3(){}");
            result[1].Assets[0].OpenStream().ReadAsString().ShouldEqual("function asset2(){}function asset1(){}");
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
