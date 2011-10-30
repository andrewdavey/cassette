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
        [Fact]
        public void CreateBundle_ReturnsHtmlTemplateBundleWithPathSet()
        {
            var factory = new HtmlTemplateBundleFactory();
            var bundle = factory.CreateBundle(
                "~/test",
                Enumerable.Empty<IFile>(),
                new BundleDescriptor { AssetFilenames = { "*" } }
            );
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void GivenBundleDescriptorWithOnlyWildcardFilename_WhenCreateBundle_ThenReturnBundleWithAllAssets()
        {
            var factory = new HtmlTemplateBundleFactory();
            var file = StubFile("~/test/file.htm");
            var files = new[] { file };
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "*" }
            };

            var bundle = factory.CreateBundle("~/test", files, bundleDescriptor);

            bundle.Assets[0].SourceFile.ShouldBeSameAs(file);
        }

        [Fact]
        public void GivenBundleDescriptorWithOnlyExplicitFilename_WhenCreateBundle_ThenReturnBundleWithOnlySpecifiedAssets()
        {
            var factory = new HtmlTemplateBundleFactory();
            var file0 = StubFile("~/test/yes.htm");
            var file1 = StubFile("~/test/no.htm");
            var files = new[] { file0, file1 };
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "~/test/yes.htm" }
            };

            var bundle = factory.CreateBundle("~/test", files, bundleDescriptor);

            bundle.Assets[0].SourceFile.ShouldBeSameAs(file0);
            bundle.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GivenBundleDescriptorWithExplicitFilenamesThenWildcard_WhenCreateBundle_ThenReturnBundleWithSpecifiedAssetsThenAllTheRemainingAssets()
        {
            var factory = new HtmlTemplateBundleFactory();
            var file0 = StubFile("~/test/0.htm");
            var file1 = StubFile("~/test/1.htm");
            var file2 = StubFile("~/test/2.htm");
            var file3 = StubFile("~/test/3.htm");
            var files = new[] { file0, file1, file2, file3 };
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "~/test/3.htm", "~/test/1.htm", "*" }
            };

            var bundle = factory.CreateBundle("~/test", files, bundleDescriptor);

            bundle.Assets.Count.ShouldEqual(4);
            bundle.Assets[0].SourceFile.ShouldBeSameAs(file3);
            bundle.Assets[1].SourceFile.ShouldBeSameAs(file1);
            new HashSet<IFile>(bundle.Assets.Skip(2).Select(a => a.SourceFile))
                .SetEquals(new[] { file0, file2 })
                .ShouldBeTrue();
        }

        [Fact]
        public void GivenBundleDescriptorWithExplicitFilename_WhenCreateBundle_ThenBundleIsSorted()
        {
            var factory = new HtmlTemplateBundleFactory();
            var file = StubFile("~/test/file.htm");
            var files = new[] { file };
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "~/test/file.htm" }
            };

            var bundle = factory.CreateBundle("~/test", files, bundleDescriptor);

            bundle.IsSorted.ShouldBeTrue();
        }

        [Fact]
        public void GivenBundleDescriptorWithOnlyWildcard_WhenCreateBundle_ThenBundleIsSortedIsFalse()
        {
            var factory = new HtmlTemplateBundleFactory();
            var file = StubFile("~/test/file.htm");
            var files = new[] { file };
            var bundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "*" }
            };

            var bundle = factory.CreateBundle("~/test", files, bundleDescriptor);

            bundle.IsSorted.ShouldBeFalse();
        }

        [Fact]
        public void GivenBundleDescriptorWithNoFilenames_WhenCreateBundle_ThenBundleIsSorted()
        {
            var factory = new HtmlTemplateBundleFactory();
            var bundleDescriptor = new BundleDescriptor();

            var bundle = factory.CreateBundle("~/test", new IFile[0], bundleDescriptor);

            bundle.IsSorted.ShouldBeTrue();
        }

        IFile StubFile(string path)
        {
            var file = new Mock<IFile>();
            file.SetupGet(f => f.FullPath).Returns(path);
            return file.Object;
        }
    }
}