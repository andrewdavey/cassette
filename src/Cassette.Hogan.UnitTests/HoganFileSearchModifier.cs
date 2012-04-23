using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HoganFileSearchModifier_Tests
    {
        [Fact]
        public void ModifyAddsMustacheToPattern()
        {
            AssertPatternContains("*.mustache");
        }

        [Fact]
        public void ModifyAddsJstToPattern()
        {
            AssertPatternContains("*.jst");
        }

        [Fact]
        public void ModifyAddsTmplToPattern()
        {
            AssertPatternContains("*.tmpl");
        }

        void AssertPatternContains(string filePattern)
        {
            var modifier = new HoganFileSearchModifier();
            var fileSearch = new FileSearch();
            modifier.Modify(fileSearch);
            filePattern.ShouldContain(filePattern);
        }
    }
}