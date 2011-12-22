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

        [Fact]
        public void NetworkSharePathRetainsLeadingDoubleSlash()
        {
            PathUtilities.NormalizePath(@"\\mbp\Users").ShouldEqual(@"\\mbp\Users");
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

