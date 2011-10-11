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
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class PerFileSource_Tests : IDisposable
    {
        readonly DirectoryInfo root;
        readonly IDirectory fileSystem;

        public PerFileSource_Tests()
        {
            root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            fileSystem = new FileSystemDirectory(root.FullName);
        }

        [Fact]
        public void GivenBasePathIsEmptyString_GetBundlesReturnsBundleForFile()
        {
            File.WriteAllText(Path.Combine(root.FullName, "test.js"), "");

            var bundles = GetBundles(new PerFileSource<Bundle>(""));

            bundles[0].Path.ShouldEqual("~/test");
        }

        [Fact]
        public void GivenBasePathIsAppRoot_ThenGetBundlesReturnsBundleForFile()
        {
            File.WriteAllText(Path.Combine(root.FullName, "test.js"), "");

            var bundles = GetBundles(new PerFileSource<Bundle>("~"));

            bundles[0].Path.ShouldEqual("~/test");
        }

        [Fact]
        public void GivenBasePathIsAppRootSlash_ThenGetBundlesReturnsBundleForFile()
        {
            File.WriteAllText(Path.Combine(root.FullName, "test.js"), "");

            var bundles = GetBundles(new PerFileSource<Bundle>("~/"));

            bundles[0].Path.ShouldEqual("~/test");
        }

        [Fact]
        public void GetBundlesReturnsBundleWithSingleAssetForTheFile()
        {
            File.WriteAllText(Path.Combine(root.FullName, "test.js"), "");

            var bundles = GetBundles(new PerFileSource<Bundle>(""));

            bundles[0].Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GetBundlesReturnsBundleWithSingleAssetWithEmptySourceFilename()
        {
            File.WriteAllText(Path.Combine(root.FullName, "test.js"), "");

            var bundles = GetBundles(new PerFileSource<Bundle>(""));

            bundles[0].Assets[0].SourceFilename.ShouldEqual("~/test.js");
        }

        [Fact]
        public void WhenBasePathSet_ThenGetBundlesReturnsBundleForFile()
        {
            root.CreateSubdirectory("path");
            File.WriteAllText(Path.Combine(root.FullName, "path\\test.js"), "");

            var bundles = GetBundles(new PerFileSource<Bundle>("path"));

            bundles[0].Path.ShouldEqual("~/path/test");
        }

        [Fact]
        public void GivenFileInSubDirectory_ThenGetBundlesReturnsBundleForFile()
        {
            root.CreateSubdirectory("path\\sub");
            File.WriteAllText(Path.Combine(root.FullName, "path\\sub\\test.js"), "");

            var bundles = GetBundles(new PerFileSource<Bundle>("path"));

            bundles[0].Path.ShouldEqual("~/path/sub/test");
            bundles[0].Assets[0].SourceFilename.ShouldEqual("~/path/sub/test.js");
        }


        [Fact]
        public void GivenSearchingOnlyTopLevelAndFileInSubDirectory_ThenGetBundlesIgnoresFile()
        {
            root.CreateSubdirectory("path\\sub");
            File.WriteAllText(Path.Combine(root.FullName, "path\\sub\\test.js"), "");

            var bundles = GetBundles(new PerFileSource<Bundle>("path")
            {
                SearchOption = SearchOption.TopDirectoryOnly
            });

            bundles.Length.ShouldEqual(0);
        }

        [Fact]
        public void WhenAFilePatternSet_ThenGetBundlesReturnsBundleForEachFileMatchingFilePattern()
        {
            File.WriteAllText(Path.Combine(root.FullName, "test1.js"), "");
            File.WriteAllText(Path.Combine(root.FullName, "test2.js"), "");
            File.WriteAllText(Path.Combine(root.FullName, "test3.txt"), "");

            var bundles = GetBundles(
                new PerFileSource<Bundle>("")
                {
                    FilePattern = "*.js"
                }
            );

            bundles.Length.ShouldEqual(2);
            bundles[0].Path.ShouldEqual("~/test1");
            bundles[1].Path.ShouldEqual("~/test2");
        }

        [Fact]
        public void WhenAMultipleFilePatternSet_ThenGetBundlesReturnsBundleForEachFileMatchingFilePattern()
        {
            File.WriteAllText(Path.Combine(root.FullName, "test1.js"), "");
            File.WriteAllText(Path.Combine(root.FullName, "test2.coffee"), "");

            var bundles = GetBundles(
                new PerFileSource<Bundle>("")
                {
                    FilePattern = "*.js;*.coffee"
                }
            );

            bundles.Length.ShouldEqual(2);
            bundles[0].Path.ShouldEqual("~/test1");
            bundles[1].Path.ShouldEqual("~/test2");
        }

        [Fact]
        public void WhenExcludeSet_ThenGetBundlesDoesNotCreateBundleForFileThatMatchesRegex()
        {
            File.WriteAllText(Path.Combine(root.FullName, "test1.js"), "");

            var bundles = GetBundles(
                new PerFileSource<Bundle>("")
                {
                    Exclude = new Regex("test")
                }
            );

            bundles.ShouldBeEmpty();
        }

        [Fact]
        public void WhenGetBundles_ThenBundleAssetCanReferenceOtherFiles()
        {
            File.WriteAllText(Path.Combine(root.FullName, "test1.js"), "");
            var bundles = GetBundles(
                new PerFileSource<Bundle>("")
            );
            var asset = bundles[0].Assets[0];

            asset.AddReference("other.js", 1);

        }

        Bundle[] GetBundles(PerFileSource<Bundle> source)
        {
            var application = StubApplication();
            var factory = StubBundleFactory();
            return source.GetBundles(factory, application).ToArray();
        }

        IBundleFactory<Bundle> StubBundleFactory()
        {
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory
                .Setup(f => f.CreateBundle(It.IsAny<string>()))
                .Returns<string>(path => new Bundle(path));
            return factory.Object;
        }

        ICassetteApplication StubApplication()
        {
            var application = new Mock<ICassetteApplication>();
            application
                .Setup(a => a.RootDirectory)
                .Returns(fileSystem);
            return application.Object;
        }

        public void Dispose()
        {
            root.Delete(true);
        }
    }
}

