using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Cassette.Utilities;
using Microsoft.CSharp;
using Should;
using Xunit;

namespace Cassette.MSBuild
{
    public class CreateBundlesWithRawFilesTests : IDisposable
    {
        readonly TempDirectory root;

        public CreateBundlesWithRawFilesTests()
        {
            root = new TempDirectory();

            CreateDirectory("source");
            CreateDirectory("source\\bin");
            CreateDirectory("output");

            WriteFile("source\\test.css", @"p { background: url(test.png); } .other { background: url(notfound.png); }");
            WriteFile("source\\test.png", "image");

            var configurationDll = CompileConfigurationDll();

            File.Move(configurationDll, Path.Combine(root, "source", "bin", "test.dll"));
            File.Copy("Cassette.dll", Path.Combine(root, "source", "bin", "Cassette.dll"));
            File.Copy("Cassette.pdb", Path.Combine(root, "source", "bin", "Cassette.pdb"));
            File.Copy("AjaxMin.dll", Path.Combine(root, "source", "bin", "AjaxMin.dll"));

            var command = new CreateBundlesCommand(
                Path.Combine(root, "source"), 
                Path.Combine(root, "source", "bin"),
                Path.Combine(root, "output"),
                true
            );
            CreateBundlesCommand.ExecuteInSeparateAppDomain(command);
        }

        string CompileConfigurationDll()
        {
            using (var provider = new CSharpCodeProvider())
            {
                var result = provider.CompileAssemblyFromSource(new CompilerParameters(new[] { "Cassette.dll" }, "test.dll"),
@"using Cassette;
using Cassette.Stylesheets;
public class Configuration : IConfiguration<BundleCollection>
{
    public void Configure(BundleCollection bundles)
    {
        bundles.Add<StylesheetBundle>(""~"");
    }
}
");
                var error = string.Join("\r\n", result.Errors.Cast<CompilerError>().Select(e => e.ErrorText));
                if (error.Length > 0) throw new Exception(error);
                return result.PathToAssembly;
            }
        }

        [Fact]
        public void ImageFileIsCopiedToOutput()
        {
            var imageOutputFilename = Path.Combine(root, "output", "file", "test-" + HashFileContent("test.png") + ".png");
            File.Exists(imageOutputFilename).ShouldBeTrue();
        }

        string HashFileContent(string filename)
        {
            using (var file = File.OpenRead(Path.Combine(root, "source", filename)))
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(file).ToHexString();
            }
        }

        void WriteFile(string filename, string content)
        {
            var fullPath = Path.Combine(root, filename);
            var directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, content);
        }

        void CreateDirectory(string path)
        {
            Directory.CreateDirectory(Path.Combine(root, path));
        }

        public void Dispose()
        {
            root.Dispose();
        }
    }
}