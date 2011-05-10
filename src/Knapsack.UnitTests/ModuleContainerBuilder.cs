using System;
using System.IO;
using System.IO.IsolatedStorage;
using Knapsack.CoffeeScript;
using Should;
using Xunit;

namespace Knapsack
{
    // Warning: These are not strictly "unit" tests given the real file system access!
    // However, they still run fast and creating a whole separate project for them seems
    // like overkill at the moment.

    public class ModuleContainerBuilder_Build : IDisposable
    {
        IsolatedStorageFile storage;
        string rootDirectory;
        ModuleContainer container;

        public ModuleContainerBuilder_Build()
        {
            storage = IsolatedStorageFile.GetUserStoreForAssembly();
            rootDirectory = Path.GetFullPath(Guid.NewGuid().ToString());

            // Create a fake set of scripts in modules.
            Directory.CreateDirectory(Path.Combine(rootDirectory, "lib"));
            File.WriteAllText(Path.Combine(rootDirectory, "lib", "jquery.js"), 
                "function jQuery(){}");
            File.WriteAllText(Path.Combine(rootDirectory, "lib", "knockout.js"), 
                "function knockout(){}");

            Directory.CreateDirectory(Path.Combine(rootDirectory, "app"));
            File.WriteAllText(Path.Combine(rootDirectory, "app", "widgets.js"), 
                "/// <reference path=\"../lib/jquery.js\"/>\r\n/// <reference path=\"../lib/knockout.js\"/>\r\nfunction widgets(){}");
            File.WriteAllText(Path.Combine(rootDirectory, "app", "main.js"), 
                "/// <reference path=\"widgets.js\"/>\r\nfunction main() {}");

            var builder = new ModuleContainerBuilder(storage, rootDirectory, new CoffeeScriptCompiler(File.ReadAllText));
            builder.AddModule("lib");
            builder.AddModule("app");
            container = builder.Build();
        }

        [Fact]
        public void Container_contains_lib()
        {
            container.Contains("lib");
        }

        [Fact]
        public void Container_contains_app()
        {
            container.Contains("app");
        }

        [Fact]
        public void Module_lib_has_2_scripts()
        {
            container.FindModule("lib").Scripts.Length.ShouldEqual(2);
        }

        [Fact]
        public void Module_app_references_module_lib()
        {
            container.FindModule("app").References[0].ShouldEqual("lib");
        }

        public void Dispose()
        {
            Directory.Delete(rootDirectory, true);

            if (storage != null)
            {
                storage.Remove();
                storage.Dispose();
            }
        }
    }

    public class ModuleContainerBuilder_AddModuleForEachSubdirectoryOf
    {
        readonly IsolatedStorageFile storage;
        readonly string rootDirectory;
        readonly ModuleContainer container;

        public ModuleContainerBuilder_AddModuleForEachSubdirectoryOf()
        {
            storage = IsolatedStorageFile.GetUserStoreForAssembly();
            rootDirectory = Path.GetFullPath(Guid.NewGuid().ToString());

            // Create a fake set of scripts in modules.
            Directory.CreateDirectory(Path.Combine(rootDirectory, "lib"));
            File.WriteAllText(Path.Combine(rootDirectory, "lib", "jquery.js"),
                "function jQuery(){}");
            File.WriteAllText(Path.Combine(rootDirectory, "lib", "knockout.js"),
                "function knockout(){}");

            Directory.CreateDirectory(Path.Combine(rootDirectory, "app"));
            File.WriteAllText(Path.Combine(rootDirectory, "app", "widgets.js"),
                "/// <reference path=\"../lib/jquery.js\"/>\r\n/// <reference path=\"../lib/knockout.js\"/>\r\nfunction widgets(){}");
            File.WriteAllText(Path.Combine(rootDirectory, "app", "main.js"),
                "/// <reference path=\"widgets.js\"/>\r\nfunction main() {}");

            var builder = new ModuleContainerBuilder(storage, rootDirectory, new CoffeeScriptCompiler(File.ReadAllText));
            builder.AddModuleForEachSubdirectoryOf("");
            container = builder.Build();
        }

        [Fact]
        public void Container_has_2_modules()
        {
            container.Contains("lib").ShouldBeTrue();
            container.Contains("app").ShouldBeTrue();
        }
    }
}
