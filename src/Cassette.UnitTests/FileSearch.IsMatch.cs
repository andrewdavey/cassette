using System.Text.RegularExpressions;
using Should;
using Xunit;

namespace Cassette
{
    public class FileSearchIsMatchTests
    {
        [Fact]
        public void GivenPatternIsJs_ThenIsMatchJsFilenameReturnsTrue()
        {
            var fileSearch = new FileSearch { Pattern = "*.js" };
            fileSearch.IsMatch("~/test.js").ShouldBeTrue();
        }

        [Fact]
        public void GivenPatternIsJs_ThenIsMatchDifferentlyCasedJsFilenameReturnsTrue()
        {
            var fileSearch = new FileSearch { Pattern = "*.js" };
            fileSearch.IsMatch("~/TEST.JS").ShouldBeTrue();
        }

        [Fact]
        public void GivenPatternIsJsAndCoffee_ThenIsMatchJsFilenameReturnsTrue()
        {
            var fileSearch = new FileSearch { Pattern = "*.js;*.coffee" };
            fileSearch.IsMatch("~/test.js").ShouldBeTrue();
        }

        [Fact]
        public void GivenPatternIsJsAndCoffee_ThenIsMatchCoffeeFilenameReturnsTrue()
        {
            var fileSearch = new FileSearch { Pattern = "*.js;*.coffee" };
            fileSearch.IsMatch("~/test.coffee").ShouldBeTrue();
        }

        [Fact]
        public void GivenPatternIsAsterisk_ThenIsMatchReturnsTrue()
        {
            var fileSearch = new FileSearch { Pattern = "*" };
            fileSearch.IsMatch("~/test.js").ShouldBeTrue();
        }

        [Fact]
        public void GivenPatternIsJsAndExcludeVsdocJs_ThenIsMatchVsdocJsFileReturnsFalse()
        {
            var fileSearch = new FileSearch
            {
                Pattern = "*.js",
                Exclude = new Regex("-vsdoc.js")
            };
            fileSearch.IsMatch("test-vsdoc.js").ShouldBeFalse();
        }
    }
}