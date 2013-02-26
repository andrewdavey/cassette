using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Cassette.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CSharp;
using Should;
using Xunit;

namespace Cassette.MSBuild
{
    [Serializable]
    public class BuildEngineStub : IBuildEngine
    {
        public BuildEngineStub()
        {
            BuildErrorEventArgs = new List<BuildErrorEventArgs>();
            BuildWarningEventArgs = new List<BuildWarningEventArgs>();
            BuildMessageEventArgs = new List<BuildMessageEventArgs>();
            CustomBuildEventArgs = new List<CustomBuildEventArgs>();
        }

        public List<BuildErrorEventArgs> BuildErrorEventArgs { get; set; }
        public List<BuildWarningEventArgs> BuildWarningEventArgs { get; set; }
        public List<BuildMessageEventArgs> BuildMessageEventArgs { get; set; }
        public List<CustomBuildEventArgs> CustomBuildEventArgs { get; set; }

        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            BuildErrorEventArgs.Add(e);
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            BuildWarningEventArgs.Add(e);
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            BuildMessageEventArgs.Add(e);
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            CustomBuildEventArgs.Add(e);
        }

        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties,
                                     IDictionary targetOutputs)
        {
            throw new System.NotImplementedException();
        }

        public bool ContinueOnError { get; private set; }
        public int LineNumberOfTaskNode { get; private set; }
        public int ColumnNumberOfTaskNode { get; private set; }
        public string ProjectFileOfTaskNode { get; private set; }
    }

    public class CreateBundlesWithRawFilesTests : IDisposable
    {
        readonly BuildEngineStub buildEngine;
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

            File.Move(configurationDll, PathUtilities.Combine(root, "source", "bin", "test.dll"));
            File.Copy("Cassette.dll", PathUtilities.Combine(root, "source", "bin", "Cassette.dll"));
            File.Copy("Cassette.pdb", PathUtilities.Combine(root, "source", "bin", "Cassette.pdb"));
            File.Copy("AjaxMin.dll", PathUtilities.Combine(root, "source", "bin", "AjaxMin.dll"));
#if NET35
            File.Copy("Iesi.Collections.dll", PathUtilities.Combine(root, "source", "bin", "Iesi.Collections.dll"));
#endif
            buildEngine = new BuildEngineStub();
            var taskLoggingHelper = new TaskLoggingHelper(buildEngine, "test");

            var command = new CreateBundlesCommand(
                PathUtilities.Combine(root, "source"),
                PathUtilities.Combine(root, "source", "bin"),
                PathUtilities.Combine(root, "output"),
                true,
                taskLoggingHelper
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
                var error = string.Join("\r\n", result.Errors.Cast<CompilerError>().Select(e => e.ErrorText).ToArray());
                if (error.Length > 0) throw new Exception(error);
                return result.PathToAssembly;
            }
        }

        [Fact]
        public void ImageFileIsCopiedToOutput()
        {
            var imageOutputFilename = PathUtilities.Combine(root, "output", "file", "test-" + HashFileContent("test.png") + ".png");
            File.Exists(imageOutputFilename).ShouldBeTrue();
        }

        [Fact]
        public void MessagesAreLogged()
        {
            buildEngine.BuildMessageEventArgs.Any().ShouldBeTrue();
        }

        string HashFileContent(string filename)
        {
            using (var file = File.OpenRead(PathUtilities.Combine(root, "source", filename)))
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