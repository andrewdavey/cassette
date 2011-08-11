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

            var transformer = new CompileLessAsset(compiler, new Module("test", Mock.Of<IFileSystem>()));

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
            compiler.Setup(c => c.Compile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IFileSystem>()))
                    .Returns(compilerOutput);
            return compiler.Object;
        }
    }
}
