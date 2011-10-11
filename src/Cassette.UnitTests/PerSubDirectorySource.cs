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

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.BundleProcessing;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class PerSubDirectorySource_Test : BundleSourceTestBase
    {
        [Fact]
        public void GivenBaseDirectoryHasEmptyDirectory_ThenGetBundlesReturnsEmptyBundle()
        {
            Directory.CreateDirectory(Path.Combine(root, "scripts", "empty"));

            var source = new PerSubDirectorySource<Bundle>("scripts");
            var result = source.GetBundles(bundleFactory, application);

            var bundle = result.First();
            bundle.Assets.Count.ShouldEqual(0);
        }

        [Fact]
        public void GivenBaseDirectoryWithTwoDirectories_ThenGetBundlesReturnsTwoBundles()
        {
            GivenFiles("scripts/bundle-a/1.js", "scripts/bundle-b/2.js");

            var source = new PerSubDirectorySource<Bundle>("scripts");
            var result = source.GetBundles(bundleFactory, application);
            
            var bundles = result.ToArray();
            bundles.Length.ShouldEqual(2);
        }

        [Fact]
        public void GivenMixedFileTypes_WhenFilesFiltered_ThenGetBundlesFindsOnlyMatchingFiles()
        {
            GivenFiles("scripts/bundle-a/1.js", "scripts/bundle-a/ignored.txt");

            var source = new PerSubDirectorySource<Bundle>("scripts") { FilePattern = "*.js" };
            var result = source.GetBundles(bundleFactory, application);

            var bundle = result.First();
            bundle.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GivenAmbiguousFileFilters_ThenGetBundlesFindsFileOnlyOnce()
        {
            GivenFiles("scripts/bundle-a/1.html");

            var source = new PerSubDirectorySource<Bundle>("scripts") { FilePattern = "*.htm;*.html" };
            var result = source.GetBundles(bundleFactory, application);

            var bundle = result.First();
            bundle.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GivenFilesWeDontWantInBundle_WhenExclusionProvided_ThenGetBundlesDoesntIncludeExcludedFiles()
        {
            GivenFiles("scripts/bundle-a/1.js", "scripts/bundle-a/1-vsdoc.js");

            var source = new PerSubDirectorySource<Bundle>("scripts") { FilePattern = "*.js" };
            source.Exclude = new Regex("-vsdoc\\.js$");

            var result = source.GetBundles(bundleFactory, application);

            var bundle = result.First();
            bundle.Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GivenBaseDirectoryHasBackSlashes_ThenGetBundleReturnsBundleWithNormalizedPath()
        {
            GivenFiles("scripts/lib/bundle-a/1.js");

            var source = new PerSubDirectorySource<Bundle>("scripts\\lib\\");
            var result = source.GetBundles(bundleFactory, application);

            var bundles = result.ToArray();
            bundles[0].Path.ShouldEqual("~/scripts/lib/bundle-a");
            bundles[0].Assets[0].SourceFilename.ShouldEqual("~/scripts/lib/bundle-a/1.js");
        }

        [Fact]
        public void GivenBaseDirectoryDoesNotExist_ThenGetBundlesThrowsException()
        {
            var source = new PerSubDirectorySource<Bundle>("missing");
            Assert.Throws<DirectoryNotFoundException>(delegate
            {
                source.GetBundles(bundleFactory, application);
            });
        }

        [Fact]
        public void WhenProcessorIsSetUsingCustomizeBundle_ThenGetBundlesReturnsBundlesWithThatProcessor()
        {
            GivenFiles("scripts/bundle-a/1.js");

            var source = new PerSubDirectorySource<ScriptBundle>("scripts");
            var factory = new Mock<IBundleFactory<ScriptBundle>>();
            factory.Setup(f => f.CreateBundle(It.IsAny<string>()))
                   .Returns<string>(p => new ScriptBundle(p));
            var processor = Mock.Of<IBundleProcessor<ScriptBundle>>();

            source.CustomizeBundle = m => m.Processor = processor;

            var result = source.GetBundles(factory.Object, application);

            result.First().Processor.ShouldBeSameAs(processor);
        }
    }
}

