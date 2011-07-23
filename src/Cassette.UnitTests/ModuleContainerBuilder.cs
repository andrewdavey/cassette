using System;
using System.IO;
using System.IO.IsolatedStorage;
using Cassette.CoffeeScript;
using Should;
using Xunit;

namespace Cassette
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

            var svn = new DirectoryInfo(Path.Combine(rootDirectory, ".svn"));
            svn.Create();
            svn.Attributes |= FileAttributes.Hidden;

            Directory.CreateDirectory(Path.Combine(rootDirectory, "nojs"));
            File.WriteAllText(Path.Combine(rootDirectory, "nojs", "foo.txt"), "foo");

            var builder = new ScriptModuleContainerBuilder(storage, rootDirectory, new CoffeeScriptCompiler(File.ReadAllText));
            builder.AddModuleForEachSubdirectoryOf("", "");
            container = builder.Build();
        }

        [Fact]
        public void Container_contains_lib()
        {
            container.Contains("lib").ShouldBeTrue();
        }

        [Fact]
        public void Container_contains_app()
        {
            container.Contains("app").ShouldBeTrue();
        }

        [Fact]
        public void Hidden_directory_is_not_added()
        {
            container.Contains(".svn").ShouldBeFalse();
        }

        [Fact]
        public void Directory_with_no_asset_files_is_not_added()
        {
            container.Contains("nojs").ShouldBeFalse();
        }

        [Fact]
        public void Module_lib_has_2_scripts()
        {
            container.FindModule("lib").Assets.Length.ShouldEqual(2);
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

    public class ModuleContainerBuilder_Build_when_already_cached_in_storage : IDisposable
    {
        IsolatedStorageFile storage;
        string rootDirectory;
        ModuleContainer container;

        public ModuleContainerBuilder_Build_when_already_cached_in_storage()
        {
            storage = IsolatedStorageFile.GetUserStoreForAssembly();
            rootDirectory = Path.GetFullPath(Guid.NewGuid().ToString());

            // Create a fake set of scripts in modules.
            Directory.CreateDirectory(Path.Combine(rootDirectory, "junk"));
            File.WriteAllText(Path.Combine(rootDirectory, "junk", "test.js"), 
                "function junk(){}");
            Directory.CreateDirectory(Path.Combine(rootDirectory, "lib"));
            File.WriteAllText(Path.Combine(rootDirectory, "lib", "jquery.js"), 
                "function jQuery(){}");
            Directory.CreateDirectory(Path.Combine(rootDirectory, "app"));
            File.WriteAllText(Path.Combine(rootDirectory, "app", "widgets.js"), 
                "/// <reference path=\"../lib/jquery.js\"/>\r\nfunction widgets(){}");
            File.WriteAllText(Path.Combine(rootDirectory, "app", "main.js"), 
                "/// <reference path=\"widgets.js\"/>\r\nfunction main() {}");

            // Create the "old" continer.
            var builder = new ScriptModuleContainerBuilder(storage, rootDirectory, new CoffeeScriptCompiler(File.ReadAllText));
            builder.AddModule("lib", null);
            builder.AddModule("app", null);
            builder.AddModule("junk", null);
            var oldContainer = builder.Build();
            oldContainer.UpdateStorage("scripts.xml");

            // Now simulate changes to the modules.
            File.WriteAllText(Path.Combine(rootDirectory, "lib", "knockout.js"), 
                "function knockout(){}");
            File.WriteAllText(Path.Combine(rootDirectory, "app", "widgets.js"), 
                "/// <reference path=\"../lib/jquery.js\"/>\r\n/// <reference path=\"../lib/knockout.js\"/>\r\nfunction widgets(){}");
            // Build the updated container to excerise container manifest loading and differencing.
            builder = new ScriptModuleContainerBuilder(storage, rootDirectory, new CoffeeScriptCompiler(File.ReadAllText));
            builder.AddModule("lib", null);
            builder.AddModule("app", null);
            container = builder.Build();
            container.UpdateStorage("scripts.xml");
        }

        [Fact]
        public void modules_that_are_no_longer_used_are_removed()
        {
            container.Contains("junk").ShouldBeFalse();
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

    public class ModuleContainerBuilder_AddModuleForEachSubdirectoryOf : IDisposable
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

            var builder = new ScriptModuleContainerBuilder(storage, rootDirectory, new CoffeeScriptCompiler(File.ReadAllText));
            builder.AddModuleForEachSubdirectoryOf("", "");
            container = builder.Build();
        }

        [Fact]
        public void Container_has_2_modules()
        {
            container.Contains("lib").ShouldBeTrue();
            container.Contains("app").ShouldBeTrue();
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
}
