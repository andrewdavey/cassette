using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Cassette.CoffeeScript;
using Xunit;
using System.IO;

namespace Cassette
{
    public class CompileCoffeeScript_Tests
    {
        public CompileCoffeeScript_Tests()
        {
            var compiler = new Mock<ICoffeeScriptCompiler>();
            step = new CompileCoffeeScript(compiler.Object);
        }

        readonly CompileCoffeeScript step;

        [Fact]
        public void WhenProcessModule_CoffeeScriptAssetIsCompiled()
        {
            throw new NotImplementedException();
        }

    }
}
