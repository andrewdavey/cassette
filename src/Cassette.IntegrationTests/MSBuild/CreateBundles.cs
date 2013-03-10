using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Cassette.Caching;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.TinyIoC;
using Microsoft.Build.Framework;
using Moq;
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

    public class GivenConfigurationClassInAssembly_WhenExecute : IDisposable
    {
        readonly TempDirectory path;
        readonly string cachePath;
        readonly string originalDirectory;

        public GivenConfigurationClassInAssembly_WhenExecute()
        {
            originalDirectory = Environment.CurrentDirectory;
            path = new TempDirectory();

            var assemblyPath = Path.Combine(path, "Test.dll");
            BundleConfiguration.GenerateAssembly(assemblyPath);

            File.WriteAllText(Path.Combine(path, "test.css"), "p { background-image: url(test.png); }");
            File.WriteAllText(Path.Combine(path, "test.coffee"), "x = 1\nlog(x)");
            File.WriteAllText(Path.Combine(path, "test.png"), "");

            Environment.CurrentDirectory = path;
            cachePath = Path.Combine(path, "cache");

            var buildEngine = new BuildEngineStub();

            var task = new CreateBundles
            {
                Source = path,
                Bin = path,
                Output = cachePath,
                BuildEngine = buildEngine
            };
            try
            {
                task.Execute();
            }
            catch (Exception exception)
            {
                var t = exception.ToString();
            }
        }

        [Fact]
        public void ManifestFileSavedToOutput()
        {
            Directory.Exists(cachePath).ShouldBeTrue();
        }

        [Fact]
        public void CoffeeScriptIsCompiled()
        {
            var filename = Directory.GetFiles(Path.Combine(cachePath, "script"))[0];
            File.ReadAllText(filename).ShouldEqual("(function(){var n;n=1,log(n)}).call(this)");
        }
   
        [Fact]
        public void CssUrlIsRewrittenToBeApplicationRooted()
        {
            var passThroughModifier = new Mock<IUrlModifier>();
            passThroughModifier
                .Setup(m => m.Modify(It.IsAny<string>()))
                .Returns<string>(url => url)
                .Verifiable();

            var bundles = LoadBundlesFromManifestFile(passThroughModifier.Object);
            var content = bundles.OfType<StylesheetBundle>().First().OpenStream().ReadToEnd();

            Regex.IsMatch(content, @"url\(cassette.axd/file/test-.*?\.png\)")
                 .ShouldBeTrue("Incorrect content: " + content);

            passThroughModifier.Verify();
        }

        IEnumerable<Bundle> LoadBundlesFromManifestFile(IUrlModifier urlModifier)
        {
            var container = new TinyIoCContainer();
            container.Register(Mock.Of<IUrlGenerator>());
            var cache = new BundleCollectionCache(
                new FileSystemDirectory(cachePath), 
                b => b == "StylesheetBundle" 
                    ? (IBundleDeserializer<Bundle>)new StylesheetBundleDeserializer(urlModifier, container)
                    : new ScriptBundleDeserializer(urlModifier, container)
            );
            var result = cache.Read();
            result.IsSuccess.ShouldBeTrue();
            return result.Manifest.Bundles;
        }

        public abstract class BundleConfiguration : IConfiguration<BundleCollection>
        {
            public void Configure(BundleCollection bundles)
            {
                bundles.Add<StylesheetBundle>("~");
                bundles.Add<ScriptBundle>("~");
            }

            public static void GenerateAssembly(string fullAssemblyPath)
            {
                var directory = Path.GetDirectoryName(fullAssemblyPath);
                var name = Path.GetFileNameWithoutExtension(fullAssemblyPath);
                var assemblyName = new AssemblyName(name);
                var filename = assemblyName.Name + ".dll";

                var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save, directory);
                var module = assembly.DefineDynamicModule(assemblyName.Name, filename);
                AddSubClassOfConfiguration(module);
                assembly.Save(filename);

                var parentAssembly = typeof(BundleConfiguration).Assembly.Location;
                File.Copy(parentAssembly, Path.Combine(directory, Path.GetFileName(parentAssembly)));
                File.Copy("Cassette.dll", Path.Combine(directory, "Cassette.dll"));
                File.Copy("AjaxMin.dll", Path.Combine(directory, "AjaxMin.dll"));
#if NET35
                File.Copy("Iesi.Collections.dll", Path.Combine(directory, "Iesi.Collections.dll"));
                File.Copy("Jurassic.dll", Path.Combine(directory, "Jurassic.dll"));

#endif
                File.Copy("Cassette.CoffeeScript.dll", Path.Combine(directory, "Cassette.CoffeeScript.dll"));
                File.Copy("Cassette.Less.dll", Path.Combine(directory, "Cassette.Less.dll"));
#if !NET35
                File.Copy("Cassette.Sass.dll", Path.Combine(directory, "Cassette.Sass.dll"));
#endif
                File.Copy("Cassette.MSBuild.dll", Path.Combine(directory, "Cassette.MSBuild.dll"));
            }

            static void AddSubClassOfConfiguration(ModuleBuilder module)
            {
                var type = module.DefineType("TestBundleDefinition", TypeAttributes.Public | TypeAttributes.Class, typeof(BundleConfiguration));
                type.CreateType();
            }
        }

        public void Dispose()
        {
            Environment.CurrentDirectory = originalDirectory;
            path.Dispose();
        }
    }
}