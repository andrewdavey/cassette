using System;
using Should;
using Xunit;

namespace Cassette.Utilities
{
    public class PathUtilities_NormalizePath_Tests
    {
        [Fact]
        public void Simple_paths_are_joined()
        {
            PathUtilities.NormalizePath("c:\\", "test").ShouldEqual("c:\\test");
        }

        [Fact]
        public void Current_directory_trailing_slash_is_optional()
        {
            PathUtilities.NormalizePath("c:", "test").ShouldEqual("c:\\test");
        }

        [Fact]
        public void Double_dot_joins_parent_directory()
        {
            PathUtilities.NormalizePath("c:\\test", "..\\foo.js").ShouldEqual("c:\\foo.js");
        }

        [Fact]
        public void Single_dot_ignored()
        {
            PathUtilities.NormalizePath("c:\\test", ".\\foo.js").ShouldEqual("c:\\test\\foo.js");
        }

        [Fact]
        public void Forward_slashes_allowed()
        {
            PathUtilities.NormalizePath("c:\\test", "module/foo").ShouldEqual("c:\\test\\module\\foo");
        }

        [Fact]
        public void Leading_slash_on_relative_path_not_allowed()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                PathUtilities.NormalizePath("c:\\test", "\\foo");
            });
        }

        [Fact]
        public void Non_absolute_current_directory_throws_ArgumentException()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                PathUtilities.NormalizePath("test", "test");
            });
        }

        [Fact]
        public void Too_many_dotdots_throws_ArgumentException()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                PathUtilities.NormalizePath("c:\\test", "..\\..\\foo");
            });
        }
    }
}
