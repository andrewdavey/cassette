using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class LessFileSearchModifier_Tests
    {
        [Fact]
        public void ModifyAddsLessPattern()
        {
            var modifier = new LessFileSearchModifier();
            var fileSearch = new FileSearch();
            modifier.Modify(fileSearch);
            fileSearch.Pattern.ShouldContain("*.less");
        }
    }
}