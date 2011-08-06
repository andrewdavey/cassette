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
        public void BuildPipeline()
        {
            var root = new DirectoryInfo("..\\..\\assets\\scripts").FullName;

            var source = new ModuleSource<Module>(root, "*.js")
                .AddEachSubDirectory();

            var pipeline = new Pipeline<Module>(
                new ParseJavaScriptReferences(),
                new SortAssetsByDependency<Module>(),
                new ConcatentateAssets(),
                new MinifyAssets(new MicrosoftJavaScriptMinifier())
            );

            foreach (var module in source.CreateModules(new ScriptModuleFactory()))
	        {
                pipeline.Process(module);

                using (var reader = new StreamReader(module.Assets[0].OpenStream()))
                {
                    reader.ReadToEnd().ShouldEqual("function asset3(){}function asset2(){}" + Environment.NewLine + "function asset1(){}");
                }
            }
        }
    }
}
