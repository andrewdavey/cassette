using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;
using Cassette.Stylesheets;
using Should;
using Xunit;

namespace Cassette
{
    public class CompilingMainTwitterBootstrapLessFile
    {
        readonly IEnumerable<string> importedFilePaths;

        public CompilingMainTwitterBootstrapLessFile()
        {
            var source = File.ReadAllText("bootstrap\\bootstrap.less");
            var context = new CompileContext
            {
                RootDirectory = new FileSystemDirectory(Path.GetFullPath(".")),
                SourceFilePath = "bootstrap/bootstrap.less"
            };

            var lessCompiler = new LessCompiler();
            var result = lessCompiler.Compile(source, context);
            importedFilePaths = result.ImportedFilePaths;
        }

        [Fact]
        public void ImportedFilePathsIncludesTheOtherBootstrapFiles()
        {
            importedFilePaths.Count().ShouldEqual(34);
        }

        [Fact]
        public void ImportedFilePathsAreApplicationRelative()
        {
            importedFilePaths.First().ShouldStartWith("~/bootstrap/");
        }
    }
}