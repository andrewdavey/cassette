using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Cassette.Configuration;
using Cassette.IntegrationTests;
using Cassette.Manifests;
using Cassette.Stylesheets;
using Should;
using Xunit;

namespace Cassette.MSBuild
{
    public class GivenConfigurationClassInAssembly_WhenExecute : IDisposable
    {
        readonly TempDirectory path;
        readonly string manifestFilename;

        public GivenConfigurationClassInAssembly_WhenExecute()
        {
            path = new TempDirectory();

            var assemblyPath = Path.Combine(path, "Test.dll");
            Configuration.GenerateAssembly(assemblyPath);

            File.WriteAllText(Path.Combine(path, "test.css"), "p { background-image: url(test.png); }");
            File.WriteAllText(Path.Combine(path, "test.png"), "");

            using (var container = new AppDomainInstance<CreateBundles>())
            {
                var task = container.Value;
                task.Assembly = assemblyPath;
                manifestFilename = Path.Combine(path, "cassette.xml");
                task.SourceDir = path;
                task.Output = manifestFilename;
                task.Execute();
            }
        }

        [Fact]
        public void ManifestFileSavedToOutput()
        {
            File.Exists(manifestFilename).ShouldBeTrue();
        }

        [Fact]
        public void CssUrlIsRewrittenToBeApplicationRooted()
        {
            var bundles = LoadBundlesFromManifestFile();
            var content = bundles.First().OpenStream().ReadToEnd();

            Regex.IsMatch(content, @"url\(/_cassette/file/test_[a-z0-9]+\.png\)").ShouldBeTrue();
        }

        IEnumerable<Bundle> LoadBundlesFromManifestFile()
        {
            using (var file = File.OpenRead(manifestFilename))
            {
                var reader = new CassetteManifestReader(file);
                return reader.Read().CreateBundles();
            }
        }

        class AppDomainInstance<T> : IDisposable
            where T : MarshalByRefObject
        {
            readonly AppDomain domain;
            readonly T value;

            public AppDomainInstance()
            {
                domain = AppDomain.CreateDomain("temp");
                value = (T)domain.CreateInstanceFromAndUnwrap(typeof(T).Assembly.Location, typeof(T).FullName);
            }

            public T Value
            {
                get { return value; }
            }

            public void Dispose()
            {
                AppDomain.Unload(domain);
            }
        }

        public class Configuration : ICassetteConfiguration
        {
            public void Configure(BundleCollection bundles, CassetteSettings settings)
            {
                bundles.Add<StylesheetBundle>("~");
            }

            public static void GenerateAssembly(string fullAssemblyPath)
            {
                var name = Path.GetFileNameWithoutExtension(fullAssemblyPath);
                var assemblyName = new AssemblyName(name);
                var filename = assemblyName.Name + ".dll";

                var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
                var module = assembly.DefineDynamicModule(assemblyName.Name, filename);
                AddSubClassOfConfiguration(module);
                assembly.Save(filename);

                File.Copy(filename, fullAssemblyPath);
                File.Delete(filename);

                var parentAssembly = typeof(Configuration).Assembly.Location;
                File.Copy(parentAssembly, Path.Combine(Path.GetDirectoryName(fullAssemblyPath), Path.GetFileName(parentAssembly)));
            }

            static void AddSubClassOfConfiguration(ModuleBuilder module)
            {
                var type = module.DefineType("TestConfiguration", TypeAttributes.Public | TypeAttributes.Class, typeof(Configuration));
                type.CreateType();
            }
        }

        public void Dispose()
        {
            path.Dispose();
        }
    }
}