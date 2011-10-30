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

using System.Collections.Generic;
using System.Linq;
using Moq;
using Should;
using Xunit;
using Cassette.IO;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateBundleFactory_Tests
    {
        readonly HtmlTemplateBundleFactory factory = new HtmlTemplateBundleFactory();
        readonly List<IFile> allFiles = new List<IFile>();

        void FilesExist(params string[] paths)
        {
            foreach (var path in paths)
            {
                var file = new Mock<IFile>();
                file.SetupGet(f => f.FullPath).Returns(path);
                allFiles.Add(file.Object);
            }
        }

        [Fact]
        public void CreateBundle_ReturnsHtmlTemplateBundleWithPathSet()
        {
            var bundle = factory.CreateBundle(
                "~/test",
                allFiles,
                new BundleDescriptor { AssetFilenames = { "*" } }
            );
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void GivenBundleDescriptorWithOnlyWildcardFilename_WhenCreateBundle_ThenReturnBundleWithAllAssets()
        {
            FilesExist("~/test/file.htm");
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "*" }
            };

            var bundle = factory.CreateBundle("~/test", allFiles, bundleDescriptor);

            bundle.Assets[0].SourceFile.ShouldBeSameAs(allFiles[0]);
        }

        [Fact]
        public void GivenBundleDescriptorWithOnlyExplicitFilename_WhenCreateBundle_ThenReturnBundleWithOnlySpecifiedAssets()
        {
            FilesExist("~/test/yes.htm", "~/test/no.htm");
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "~/test/yes.htm" }
            };

            var bundle = factory.CreateBundle("~/test", allFiles, bundleDescriptor);

            bundle.Assets[0].SourceFile.ShouldBeSameAs(allFiles[0]);
            bundle.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GivenBundleDescriptorWithExplicitFilenamesThenWildcard_WhenCreateBundle_ThenReturnBundleWithSpecifiedAssetsThenAllTheRemainingAssets()
        {
            FilesExist(Enumerable.Range(0, 4).Select(i => "~/test/" + i + ".htm").ToArray());

            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "~/test/3.htm", "~/test/1.htm", "*" }
            };

            var bundle = factory.CreateBundle("~/test", allFiles, bundleDescriptor);

            bundle.Assets.Count.ShouldEqual(4);
            bundle.Assets[0].SourceFile.ShouldBeSameAs(allFiles[3]);
            bundle.Assets[1].SourceFile.ShouldBeSameAs(allFiles[1]);
            new HashSet<IFile>(bundle.Assets.Skip(2).Select(a => a.SourceFile))
                .SetEquals(new[] { allFiles[0], allFiles[2] })
                .ShouldBeTrue();
        }

        [Fact]
        public void GivenBundleDescriptorWithExplicitFilename_WhenCreateBundle_ThenBundleIsSorted()
        {
            FilesExist("~/test/file.htm");
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "~/test/file.htm" }
            };

            var bundle = factory.CreateBundle("~/test", allFiles, bundleDescriptor);

            bundle.IsSorted.ShouldBeTrue();
        }

        [Fact]
        public void GivenBundleDescriptorWithOnlyWildcard_WhenCreateBundle_ThenBundleIsSortedIsFalse()
        {
            FilesExist("~/test/file.htm");
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "*" }
            };

            var bundle = factory.CreateBundle("~/test", allFiles, bundleDescriptor);

            bundle.IsSorted.ShouldBeFalse();
        }

        [Fact]
        public void GivenBundleDescriptorWithNoFilenames_WhenCreateBundle_ThenBundleIsSorted()
        {
            var bundleDescriptor = new BundleDescriptor();

            var bundle = factory.CreateBundle("~/test", new IFile[0], bundleDescriptor);

            bundle.IsSorted.ShouldBeTrue();
        }

        [Fact]
        public void GivenSubDirectoryAsterisks_WhenCreateBundle_ThenFilesFromSubDirectoriesAreIncluded()
        {
            // Thanks to maniserowicz for this idea

            FilesExist("~/shared/shared-test1.htm", "~/shared/shared-test2.htm", "~/app/app-test1.htm", "~/app/app-test2.htm");
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "~/shared/*", "~/app/*" }
            };

            var bundle = factory.CreateBundle("~", allFiles, bundleDescriptor);
            
            bundle.Assets.Select(a => a.SourceFile.FullPath)
                .SequenceEqual(new[] { "~/shared/shared-test1.htm", "~/shared/shared-test2.htm", "~/app/app-test1.htm", "~/app/app-test2.htm" })
                .ShouldBeTrue();
        }

        [Fact]
        public void GivenExplicitSubDirFileAndThenSubDirAsterisk_WhenCreateBundle_ThenExplicitFileNotAddedTwice()
        {
            // Thanks to maniserowicz for this idea

            FilesExist("~/shared/shared-test1.htm", "~/shared/shared-test2.htm", "~/app/app-test1.htm", "~/app/app-test2.htm");
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "~/shared/shared-test2.htm", "~/shared/*", "~/app/*" }
            };

            var bundle = factory.CreateBundle("~", allFiles, bundleDescriptor);

            bundle.Assets.Select(a => a.SourceFile.FullPath)
                .SequenceEqual(new[] { "~/shared/shared-test2.htm", "~/shared/shared-test1.htm", "~/app/app-test1.htm", "~/app/app-test2.htm" })
                .ShouldBeTrue();
        }

        [Fact]
        public void GivenSubDirAsteriskAndTopLevelAsterisk_WhenCreateBundle_ThenSubDirFilesNotAddedTwice()
        {
            FilesExist("~/shared/a.htm", "~/shared/b.htm", "~/c.htm");
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "~/shared/*", "*" }
            };

            var bundle = factory.CreateBundle("~", allFiles, bundleDescriptor);

            bundle.Assets.Select(a => a.SourceFile.FullPath)
                .SequenceEqual(new[] { "~/shared/a.htm", "~/shared/b.htm", "~/c.htm" })
                .ShouldBeTrue();
        }
    }
}