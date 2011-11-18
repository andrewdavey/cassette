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
using Should;
using Xunit;

namespace Cassette.Utilities
{
    public class PathUtilities_NormalizePath_Tests
    {
        [Fact]
        public void NormalizedPathRemainsNormalized()
        {
            PathUtilities.NormalizePath("foo/bar").ShouldEqual("foo/bar");
        }

        [Fact]
        public void DoubleDotNavigatesUpToParent()
        {
            PathUtilities.NormalizePath("foo/../test").ShouldEqual("test");
        }

        [Fact]
        public void SingleDotIsIgnored()
        {
            PathUtilities.NormalizePath("test/./foo.js").ShouldEqual("test/foo.js");
        }

        [Fact]
        public void BackSlashesConvertedToForwardSlashes()
        {
            PathUtilities.NormalizePath("test\\bundle\\foo").ShouldEqual("test/bundle/foo");
        }

        [Fact]
        public void TooManyDotDotsThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                PathUtilities.NormalizePath("test\\..\\..\\foo");
            });
        }
    }

    public class PathUtilities_AppRelative_Tests
    {
        [Fact]
        public void PrependsTildeSlash()
        {
            PathUtilities.AppRelative("test").ShouldEqual("~/test");
        }

        [Fact]
        public void PrependsTildeWhenSlashPrefix()
        {
            PathUtilities.AppRelative("/test").ShouldEqual("~/test");
        }

        [Fact]
        public void DoesNotChangePathStartingWithTildeSlash()
        {
            PathUtilities.AppRelative("~/test").ShouldEqual("~/test");
        }

        [Fact]
        public void DoesNotChangeUrl()
        {
            PathUtilities.AppRelative("http://test.com/").ShouldEqual("http://test.com/");
        }
    }

    public class PathUtilities_PathEquals_Tests
    {
        [Fact]
        public void null_null_returns_true()
        {
            PathUtilities.PathsEqual(null, null).ShouldBeTrue();
        }

        [Fact]
        public void null_string_returns_false()
        {
            PathUtilities.PathsEqual(null, "").ShouldBeFalse();
        }

        [Fact]
        public void string_null_returns_false()
        {
            PathUtilities.PathsEqual("", null).ShouldBeFalse();
        }

        [Fact]
        public void  DifferentCasesStillEqual()
        {
            PathUtilities.PathsEqual("A", "a").ShouldBeTrue();
        }

        [Fact]
        public void DifferentSlashesStillEqual()
        {
            PathUtilities.PathsEqual("test/foo", "test\\foo").ShouldBeTrue();
        }
    }
}

