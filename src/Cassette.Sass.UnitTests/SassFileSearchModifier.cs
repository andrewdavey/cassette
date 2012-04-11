#if !NET35
using Should;
using Xunit;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class SassFileSearchModifier_Tests
    {
        readonly SassFileSearchModifier modifier = new SassFileSearchModifier();
        readonly FileSearch fileSearch = new FileSearch();

        [Fact]
        public void ModifyAddsScssPattern()
        {
            modifier.Modify(fileSearch);
            fileSearch.Pattern.ShouldContain("*.scss");
        }

        [Fact]
        public void ModifyAddsSassToPattern()
        {
            modifier.Modify(fileSearch);
            fileSearch.Pattern.ShouldContain("*.sass");
        }
    }
}
#endif