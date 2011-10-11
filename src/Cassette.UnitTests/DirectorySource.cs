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

using System.Linq;
using Should;
using Xunit;

namespace Cassette
{
    // DirectorySource shares most of it's implementation with PerSubDirectorySource.
    // So please see the PerSubDirectorySource tests for more thorough testing of this
    // common code.

    public class DirectorySource_Tests : BundleSourceTestBase
    {
        [Fact]
        public void GivenDirectoryWithFile_ThenGetBundlesReturnsBundleWithAsset()
        {
            GivenFiles("bundle-a/1.js");

            var source = new DirectorySource<Bundle>("bundle-a");

            var result = source.GetBundles(bundleFactory, application);
            result.Count().ShouldEqual(1);
            result.First().Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void GivenTwoDirectories_ThenGetBundlesReturnsTwoBundles()
        {
            GivenFiles("bundle-a/1.js", "bundle-b/2.js");

            var source = new DirectorySource<Bundle>("bundle-a", "bundle-b");

            var result = source.GetBundles(bundleFactory, application);
            result.Count().ShouldEqual(2);
        }
    }
}
