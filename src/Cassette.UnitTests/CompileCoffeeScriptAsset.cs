using System.IO;
using Cassette.CoffeeScript;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class CompileCoffeeScriptAsset_Tests
    {
        [Fact]
        public void TransformCallsCoffeeScriptCompiler()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("c:\\test.coffee");

            var sourceInput = "source-input";
            var compilerOutput = "compiler-output";
            var compiler = StubCompiler(sourceInput, compilerOutput);

            var transformer = new CompileCoffeeScriptAsset(compiler);

            var getResultStream = transformer.Transform(
                () => CreateSourceStream(sourceInput),
                asset.Object
            );

            using (var reader = new StreamReader(getResultStream()))
            {
                reader.ReadToEnd().ShouldEqual(compilerOutput);
            }
        }

        Stream CreateSourceStream(string text)
        {
            var source = new MemoryStream();
            var writer = new StreamWriter(source);
            writer.Write(text);
            writer.Flush();
            source.Position = 0;
            return source;
        }

        ICoffeeScriptCompiler StubCompiler(string expectedSourceInput, string compilerOutput)
        {
            var compiler = new Mock<ICoffeeScriptCompiler>();
            compiler.Setup(c => c.Compile(expectedSourceInput, "c:\\test.coffee"))
                    .Returns(compilerOutput);
            return compiler.Object;
        }
    }
}
