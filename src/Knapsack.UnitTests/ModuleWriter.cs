using System.Collections.Generic;
using System.IO;
using Should;
using Xunit;

namespace Knapsack
{
    public class ModuleWriter_facts
    {
        [Fact]
        public void Write_minifies_all_script_content_in_given_order()
        {
            using (var textWriter = new StringWriter())
            {
                var module = new Module("a", new[] { CreateScript("1"), CreateScript("2") }, new string[0]);
                var sources = new Dictionary<string, string>
                {
                    { "a\\1.js", "function test1 () { }" },
                    { "a\\2.js", "function test2 () { }" }
                };
                var moduleWriter = new ModuleWriter(textWriter, path => sources[path]);

                moduleWriter.Write(module);

                var output = textWriter.GetStringBuilder().ToString();
                output.ShouldEqual("function test1(){}function test2(){}");
            }
        }

        Script CreateScript(string name)
        {
            return new Script("a\\" + name + ".js", new byte[0], new string[0]);
        }
    }
}
