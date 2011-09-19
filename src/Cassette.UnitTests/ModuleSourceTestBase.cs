#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.IO;
using Cassette.IO;
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
                           .Returns(new FileSystemDirectory(root));
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

