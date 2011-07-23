using System.Collections.Generic;
using System.IO;
using Should;
using Xunit;
using Cassette.CoffeeScript;

namespace Cassette.Assets.Scripts
{
    public class ModuleWriter_facts
    {
        [Fact]
        public void Write_minifies_all_script_content_in_given_order()
        {
            using (var textWriter = new StringWriter())
            {
                var module = new Module("a", new[] { CreateScript("1"), CreateScript("2") }, new string[0], null);
                var sources = new Dictionary<string, string>
                {
                    { "a/1.js", "function test1 () { }" },
                    { "a/2.js", "function test2 () { }" }
                };
                var moduleWriter = new ScriptModuleWriter(textWriter, "", path => sources[path], null);

                moduleWriter.Write(module);

                var output = textWriter.GetStringBuilder().ToString();
                output.ShouldEqual("function test1(){}function test2(){}");
            }
        }

        [Fact]
        public void Write_compiles_coffee_script()
        {
            using (var textWriter = new StringWriter())
            {
                var module = new Module("a", new[] { new Asset("a/test.coffee", new byte[0], new string[0]) }, new string[0], null);
                var sources = new Dictionary<string, string>
                {
                    { "a/test.coffee", "x = 1" }
                };
                var moduleWriter = new ScriptModuleWriter(textWriter, "", path => sources[path], new FakeCompiler());

                moduleWriter.Write(module);

                var output = textWriter.GetStringBuilder().ToString();
                output.ShouldEqual("compiled");
            }
        }

        Asset CreateScript(string name)
        {
            return new Asset("a/" + name + ".js", new byte[0], new string[0]);
        }

        class FakeCompiler : ICoffeeScriptCompiler
        {
            public string CompileFile(string path)
            {
                return "compiled";
            }
        }

    }
}
