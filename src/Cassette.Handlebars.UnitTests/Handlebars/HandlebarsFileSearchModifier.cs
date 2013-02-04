using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HandlebarsFileSearchModifier_Tests
    {
        [Fact]
        public void ModifyAddsMustacheToPattern()
        {
            AssertPatternContains("*.handlebars");
        }

        [Fact]
        public void ModifyAddsJstToPattern()
        {
            AssertPatternContains("*.hbr");
        }

        [Fact]
        public void ModifyAddsTmplToPattern()
        {
            AssertPatternContains("*.hbt");
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