using System.IO;
using Cassette.Utilities;
using Moq;
using Xunit;
using Should;

namespace Cassette.ModuleProcessing
{
    public class CompileAsset_Tests
    {
        [Fact]
        public void TransformCallsLessCompiler()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("test.less");

            var sourceInput = "source-input";
            var compilerOutput = "compiler-output";
            var compiler = StubCompiler(sourceInput, compilerOutput);

            var transformer = new CompileAsset(compiler);

            var getResultStream = transformer.Transform(
                () => sourceInput.AsStream(),
                asset.Object
            );

            using (var reader = new StreamReader(getResultStream()))
            {
                reader.ReadToEnd().ShouldEqual(compilerOutput);
            }
        }

        ICompiler StubCompiler(string expectedSourceInput, string compilerOutput)
        {
            var compiler = new Mock<ICompiler>();
            compiler.Setup(c => c.Compile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IFileSystem>()))
                    .Returns(compilerOutput);
            return compiler.Object;
        }
    }
}
