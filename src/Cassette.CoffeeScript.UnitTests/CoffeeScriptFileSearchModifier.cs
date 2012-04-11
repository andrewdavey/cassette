using Cassette.Configuration;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class CoffeeScriptFileSearchModifier_Tests
    {
        [Fact]
        public void ModifyAddsCoffeePattern()
        {
            var modifier = new CoffeeScriptFileSearchModifier();
            var fileSearch = new FileSearch();
            modifier.Modify(fileSearch);
            fileSearch.Pattern.ShouldContain("*.coffee");
        }
    }
}