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
    class FileSystemBundleSource_Tests : IDisposable
    {
        public FileSystemBundleSource_Tests()
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

        class TestableFileSystemBundleSource : FileSystemBundleSource<Bundle>
        {
            protected override IEnumerable<string> GetBundleDirectoryPaths(ICassetteApplication application)
            {
                yield return "~/test";
            }
        }

        [Fact]
        public void GivenBundleDescriptorWithExternalUrl_WhenGetBundles_ThenResultContainsExternalBundle()
        {
            File.WriteAllText(
                Path.Combine(root.FullName, "test", "bundle.txt"),
                "[external]\nurl = http://test.com"
            );

            var factory = new Mock<IBundleFactory<Bundle>>();
            factory
                .Setup(f => f.CreateExternalBundle("~/test", It.IsAny<BundleDescriptor>()))
                .Returns(new ExternalScriptBundle("~/test", "http://test.com"));

            var source = new TestableFileSystemBundleSource();
            var bundles = source.GetBundles(factory.Object, application.Object).ToArray();

            bundles[0].ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void SearchOptionPropertyDefaultsToAllDirectories()
        {
            var source = new TestableFileSystemBundleSource();
            source.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }

        [Fact]
        public void GivenSearchOptionIsTopDirectoryOnly_WhenGetBundles_ThenFilesInSubDirectoriesAreIgnored()
        {
            root.CreateSubdirectory("test\\sub");
            File.WriteAllText(Path.Combine(root.FullName, "test", "test1.txt"), "");
            File.WriteAllText(Path.Combine(root.FullName, "test", "sub", "test2.txt"), "");
            
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory
                .Setup(f => f.CreateBundle("~/test"))
                .Returns(new Bundle("~"));

            var source = new DirectorySource<Bundle>("~/test")
            {
                SearchOption = SearchOption.TopDirectoryOnly
            };
            var bundles = source.GetBundles(factory.Object, application.Object);
            bundles.First().Assets.Count.ShouldEqual(1);
        }

        public void Dispose()
        {
            root.Delete(true);
        }
    }
}

