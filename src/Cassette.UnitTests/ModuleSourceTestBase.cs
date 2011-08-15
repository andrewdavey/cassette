using System;
using System.IO;
using Moq;

namespace Cassette
{
    // Base test class for PerSubDirectorySource and DirectorySource tests.
    // A temporary file system directory is used by tests. Although it's a bit iffy
    // to see real IO in a unit test, it's still very fast for our purposes. It's also
    // much easier than mocking out the file system!

    public abstract class ModuleSourceTestBase : IDisposable
    {
        public ModuleSourceTestBase()
        {
            root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(root);

            application = StubApplication();
            moduleFactory = StubModuleFactory();
        }

        protected readonly string root;
        protected readonly ICassetteApplication application;
        protected readonly IModuleFactory<Module> moduleFactory;

        protected void GivenFiles(params string[] filenames)
        {
            foreach (var filename in filenames)
            {
                var dir = Path.Combine(root, Path.GetDirectoryName(filename));
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(Path.Combine(root, filename), "");
            }
        }

        ICassetteApplication StubApplication()
        {
            var mockApplication = new Mock<ICassetteApplication>();
            mockApplication.Setup(app => app.RootDirectory)
                           .Returns(new FileSystem(root));
            return mockApplication.Object;
        }

        IModuleFactory<Module> StubModuleFactory()
        {
            var factory = new Mock<IModuleFactory<Module>>();
            factory.Setup(f => f.CreateModule(It.IsAny<string>()))
                   .Returns<string>(p => new Module(p));
            return factory.Object;
        }

        public void Dispose()
        {
            Directory.Delete(root, true);
        }
    }
}
