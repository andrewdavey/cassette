using System.IO;
using Cassette.Less;
using Cassette.Utilities;
using Moq;
using Xunit;
using Should;

namespace Cassette.ModuleProcessing
{
    public class CompileLessAsset_Tests
    {
        [Fact]
        public void TransformCallsLessCompiler()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("test.less");

            var sourceInput = "source-input";
            var compilerOutput = "compiler-output";
            var compiler = StubCompiler(sourceInput, compilerOutput);

            var transformer = new CompileLessAsset(compiler);

            var getResultStream = transformer.Transform(
                () => sourceInput.AsStream(),
                asset.Object
            );

            using (var reader = new StreamReader(getResultStream()))
            {
                reader.ReadToEnd().ShouldEqual(compilerOutput);
            }
        }

        ILessCompiler StubCompiler(string expectedSourceInput, string compilerOutput)
        {
            var compiler = new Mock<ILessCompiler>();
            compiler.Setup(c => c.CompileFile("test.coffee"))
                    .Returns(compilerOutput);
            return compiler.Object;
        }
    }
}
