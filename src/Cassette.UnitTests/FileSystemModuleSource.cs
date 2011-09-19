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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    class FileSystemModuleSource_Tests : IDisposable
    {
        public FileSystemModuleSource_Tests()
        {
            root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            root.CreateSubdirectory("test");
            application = new Mock<ICassetteApplication>();
            application
                .Setup(a => a.RootDirectory)
                .Returns(new FileSystemDirectory(root.FullName));
        }

        DirectoryInfo root;
        Mock<ICassetteApplication> application;

        class TestableFileSystemModuleSource : FileSystemModuleSource<Module>
        {
            protected override IEnumerable<string> GetModuleDirectoryPaths(ICassetteApplication application)
            {
                yield return "~/test";
            }
        }

        [Fact]
        public void GivenModuleDescriptorWithExternalUrl_WhenGetModules_ThenResultContainsExternalModule()
        {
            File.WriteAllText(
                Path.Combine(root.FullName, "test", "module.txt"),
                "[external]\nurl = http://test.com"
            );

            var factory = new Mock<IModuleFactory<Module>>();
            factory
                .Setup(f => f.CreateExternalModule("~/test", It.IsAny<ModuleDescriptor>()))
                .Returns(new ExternalScriptModule("~/test", "http://test.com"));

            var source = new TestableFileSystemModuleSource();
            var modules = source.GetModules(factory.Object, application.Object).ToArray();

            modules[0].ShouldBeType<ExternalScriptModule>();
        }

        [Fact]
        public void SearchOptionPropertyDefaultsToAllDirectories()
        {
            var source = new TestableFileSystemModuleSource();
            source.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }

        [Fact]
        public void GivenSearchOptionIsTopDirectoryOnly_WhenGetModules_ThenFilesInSubDirectoriesAreIgnored()
        {
            root.CreateSubdirectory("test\\sub");
            File.WriteAllText(Path.Combine(root.FullName, "test", "test1.txt"), "");
            File.WriteAllText(Path.Combine(root.FullName, "test", "sub", "test2.txt"), "");
            
            var factory = new Mock<IModuleFactory<Module>>();
            factory
                .Setup(f => f.CreateModule("~/test"))
                .Returns(new Module("~"));

            var source = new DirectorySource<Module>("~/test")
            {
                SearchOption = SearchOption.TopDirectoryOnly
            };
            var modules = source.GetModules(factory.Object, application.Object);
            modules.First().Assets.Count.ShouldEqual(1);
        }

        public void Dispose()
        {
            root.Delete(true);
        }
    }
}

