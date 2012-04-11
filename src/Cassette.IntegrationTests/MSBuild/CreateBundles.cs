using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using Cassette.Configuration;
using Cassette.IntegrationTests;
using Cassette.Manifests;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.MSBuild
{
    public class GivenConfigurationClassInAssembly_WhenExecute : IDisposable
    {
        readonly TempDirectory path;
        readonly string manifestFilename;
        readonly string originalDirectory;

        public GivenConfigurationClassInAssembly_WhenExecute()
        {
            originalDirectory = Environment.CurrentDirectory;
            path = new TempDirectory();

            var assemblyPath = Path.Combine(path, "Test.dll");
            BundleDefinition.GenerateAssembly(assemblyPath);

            File.WriteAllText(Path.Combine(path, "test.css"), "p { background-image: url(test.png); }");
            File.WriteAllText(Path.Combine(path, "test.png"), "");

            using (var container = new AppDomainInstance<CreateBundles>())
            {
                var task = container.Value;
                task.Assemblies = new[] { assemblyPath };
                manifestFilename = Path.Combine(path, "cassette.xml");
                Environment.CurrentDirectory = path;
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
            var passThroughModifier = new Mock<IUrlModifier>();
            passThroughModifier
                .Setup(m => m.Modify(It.IsAny<string>()))
                .Returns<string>(url => url)
                .Verifiable();

            var bundles = LoadBundlesFromManifestFile(passThroughModifier.Object);
            var content = bundles.First().OpenStream().ReadToEnd();

            Regex.IsMatch(content, @"url\(_cassette/file/test_[a-z0-9]+\.png\)").ShouldBeTrue();
            passThroughModifier.Verify();
        }

        IEnumerable<Bundle> LoadBundlesFromManifestFile(IUrlModifier urlModifier)
        {
            using (var file = File.OpenRead(manifestFilename))
            {
                var reader = new CassetteManifestReader(file);
                return reader.Read().CreateBundles(urlModifier);
            }
        }

        class AppDomainInstance<T> : IDisposable
            where T : MarshalByRefObject
        {
            readonly AppDomain domain;
            readonly T value;

            /// <summary>
            /// Creates a new AppDomain instance that is sandboxed with full trust permissions
            /// See: http://blogs.msdn.com/b/shawnfa/archive/2006/05/01/587654.aspx
            /// </summary>
            public AppDomainInstance()
            {
                // Create a new Sandboxed App Domain
                var pset = new PermissionSet(PermissionState.Unrestricted);

                // Full trust execution
                pset.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

                // Set base to current directory
                var setup = new AppDomainSetup
                {
                    ApplicationBase = Path.GetDirectoryName(typeof(T).Assembly.Location)
                };

                // Sandbox app domain constructor
                domain = AppDomain.CreateDomain("temp", AppDomain.CurrentDomain.Evidence, setup, pset);

                // This will not demand a FileIOPermission and is a safe way to load an assembly
                // from an app domain
                value = (T)Activator.CreateInstanceFrom(
                    domain,
                    Path.GetFileName(typeof(T).Assembly.Location),
                    typeof(T).FullName
                ).Unwrap();
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

        public class BundleDefinition : IBundleDefinition
        {
            public void AddBundles(BundleCollection bundles)
            {
                bundles.Add<StylesheetBundle>("~");
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

                var parentAssembly = typeof(BundleDefinition).Assembly.Location;
                File.Copy(parentAssembly, Path.Combine(directory, Path.GetFileName(parentAssembly)));
                File.Copy("Cassette.dll", Path.Combine(directory, "Cassette.dll"));
                File.Copy("Cassette.MSBuild.dll", Path.Combine(directory, "Cassette.MSBuild.dll"));
            }

            static void AddSubClassOfConfiguration(ModuleBuilder module)
            {
                var type = module.DefineType("TestBundleDefinition", TypeAttributes.Public | TypeAttributes.Class, typeof(BundleDefinition));
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