using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HandlebarsFileSearchModifier_Tests
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

        [Fact]
        public void ModifyAddsHbsToPattern()
        {
            
        }

        void AssertPatternContains(string filePattern)
        {
            var modifier = new HandlebarsFileSearchModifier();
            var fileSearch = new FileSearch();
            modifier.Modify(fileSearch);
            filePattern.ShouldContain(filePattern);
        }
    }
}