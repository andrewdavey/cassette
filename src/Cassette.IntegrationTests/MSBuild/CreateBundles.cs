using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Cassette.Configuration;
using Should;
using Xunit;

namespace Cassette.MSBuild
{
    public class CreateBundles_Tests
    {
        [Fact]
        public void GivenConfigurationClassInAssembly_WhenExecute_ThenManifestFileSavedToOutput()
        {
            using (var path = new TempDirectory())
            {
                var assemblyPath = Path.Combine(path, "Test.dll");
                Configuration.GenerateAssembly(assemblyPath);

                using (var container = new AppDomainInstance<CreateBundles>())
                {
                    var task = container.Value;
                    task.Assembly = assemblyPath;
                    task.Output = Path.Combine(path, "cassette.xml");
                    task.Execute();
                }

                File.Exists(Path.Combine(path, "cassette.xml")).ShouldBeTrue();
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

        class Configuration  : ICassetteConfiguration
        {
            public void Configure(BundleCollection bundles, CassetteSettings settings)
            {
            }

            public static void GenerateAssembly(string fullAssemblyPath)
            {
                var name = Path.GetFileNameWithoutExtension(fullAssemblyPath);
                var assemblyName = new AssemblyName(name);
                var filename = assemblyName.Name + ".dll";

                var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
                var module = assembly.DefineDynamicModule(assemblyName.Name, filename);
                GenerateConfigurationClass(module);
                assembly.Save(filename);

                File.Copy(filename, fullAssemblyPath);
                File.Delete(filename);
            }

            static void GenerateConfigurationClass(ModuleBuilder module)
            {
                var type = module.DefineType("Configuration", TypeAttributes.Public);
                type.AddInterfaceImplementation(typeof(ICassetteConfiguration));
                GenerateConfigureMethod(type);
                type.CreateType();
            }

            static void GenerateConfigureMethod(TypeBuilder type)
            {
                const string name = "Configure";
                var method = type.DefineMethod(
                    name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    null,
                    new[] { typeof(BundleCollection), typeof(CassetteSettings) }
                );
                var bytes = typeof(Configuration).GetMethod(name).GetMethodBody().GetILAsByteArray();
                method.CreateMethodBody(bytes, bytes.Length);
                type.DefineMethodOverride(method, typeof(ICassetteConfiguration).GetMethod(name));
            }
        }
    }
}