using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Should;

namespace Knapsack
{
    public class ReferenceBuilder_with_modules
    {
        ReferenceBuilder builder;

        public ReferenceBuilder_with_modules()
        {
            // 'b\4.js' <- 'a\1.js' <- 'a\3.js' <- 'b\2.js'
            builder = new ReferenceBuilder(
                new ModuleContainer(new[]
                {
                    new Module(
                        "a",
                        new[]
                        {
                            CreateScript("1", "a", "b\\4.js"),
                            CreateScript("2", "a", "a\\3.js"),
                            CreateScript("3", "a", "a\\1.js")
                        },
                        new [] { "b" }
                    ),
                    new Module(
                        "b", 
                        new[]
                        { 
                            CreateScript("4", "b")
                        }, 
                        new string[0]
                    )
                })
            );
        }

        Script CreateScript(string name, string module, params string[] references)
        {
            return new Script(module + "\\" + name + ".js", new byte[0], references);
        }

        [Fact]
        public void AddReference_b_4_loads_only_module_b()
        {
            builder.AddReference("b\\4.js");
            var modules = builder.GetRequiredModules().ToArray();
            modules.Length.ShouldEqual(1);
            modules[0].Path.ShouldEqual("b");
        }

        [Fact]
        public void AddReference_a_1_loads_module_a_and_b()
        {
            builder.AddReference("a\\1.js");
            var modules = builder.GetRequiredModules().ToArray();
            modules.Length.ShouldEqual(2);
            modules[0].Path.ShouldEqual("b");
            modules[1].Path.ShouldEqual("a");
        }
    }
}
