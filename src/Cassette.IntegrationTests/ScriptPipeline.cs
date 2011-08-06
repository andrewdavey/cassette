using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;
using System.Reflection;
using Cassette;
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

            var modules = source.CreateModules(new ScriptModuleFactory()).ToArray();
            foreach (var module in modules)
	        {
                pipeline.Process(module);
            }
            var container = new ModuleContainer<ScriptModule>(modules);

            var result = container.Modules.ToArray();
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
