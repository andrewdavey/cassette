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
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class AssetReference_Tests
    {
        [Fact]
        public void ConstructorAssignsProperties()
        {
            var asset = Mock.Of<IAsset>();
            var reference = new AssetReference("~/path", asset, 1, AssetReferenceType.DifferentBundle);

            reference.Path.ShouldEqual("~/path");
            reference.SourceAsset.ShouldBeSameAs(asset);
            reference.SourceLineNumber.ShouldEqual(1);
            reference.Type.ShouldEqual(AssetReferenceType.DifferentBundle);
        }

        [Fact]
        public void WhenCreateWithDifferentBundleTypeAndPathNotStartingWithTilde_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new AssetReference("fail", null, -1, AssetReferenceType.DifferentBundle)
            );
        }

        [Fact]
        public void WhenCreateWithSameBundleTypeAndPathNotStartingWithTilde_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new AssetReference("fail", null, -1, AssetReferenceType.SameBundle)
            );
        }

        [Fact]
        public void WhenCreateWithRawFilenameTypeAndPathNotStartingWithTilde_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new AssetReference("fail", null, -1, AssetReferenceType.RawFilename)
            );
        }

        [Fact]
        public void WhenCreateWithUrlTypeAndPathIsNotUrl_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new AssetReference("not-a-url", null, -1, AssetReferenceType.Url)
            );
        }
    }
}

