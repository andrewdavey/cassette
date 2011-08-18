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
            PathUtilities.NormalizePath("foo\\bar").ShouldEqual("foo\\bar");
        }

        [Fact]
        public void DoubleDotNavigatesUpToParent()
        {
            PathUtilities.NormalizePath("foo\\..\\test").ShouldEqual("test");
        }

        [Fact]
        public void SingleDotIsIgnored()
        {
            PathUtilities.NormalizePath("test\\.\\foo.js").ShouldEqual("test\\foo.js");
        }

        [Fact]
        public void Forward_slashes_allowed()
        {
            PathUtilities.NormalizePath("test/module/foo").ShouldEqual("test\\module\\foo");
        }

        [Fact]
        public void Too_many_dotdots_throws_ArgumentException()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                PathUtilities.NormalizePath("test\\..\\..\\foo");
            });
        }
    }
}
