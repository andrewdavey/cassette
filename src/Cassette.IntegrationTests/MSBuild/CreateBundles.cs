using System;
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
using Microsoft.Build.Framework;
using Moq;
using Should;
using Xunit;

namespace Cassette.MSBuild
{
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
            File.WriteAllText(Path.Combine(path, "test.coffee"), "x = 1");
            File.WriteAllText(Path.Combine(path, "test.png"), "");

            Environment.CurrentDirectory = path;
            cachePath = Path.Combine(path, "cache");

            var task = new CreateBundles
            {
                Source = path,
                Bin = path,
                Output = cachePath,
                BuildEngine = Mock.Of<IBuildEngine>()
            };
            task.Execute();
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
            File.ReadAllText(filename).ShouldEqual("(function(){var n;n=1}).call(this)");
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
            var cache = new BundleCollectionCache(
                new FileSystemDirectory(cachePath), 
                b => b == "StylesheetBundle" 
                    ? (IBundleDeserializer<Bundle>)new StylesheetBundleDeserializer(urlModifier) 
                    : new ScriptBundleDeserializer(urlModifier)
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