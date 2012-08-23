using Should;
using Xunit;

namespace Cassette.MSBuild
{
    public class UrlPlaceholderWrapper_Tests
    {
        [Fact]
        public void ModifyWrapsUrlWithPlaceholder()
        {
            var modifier = new UrlPlaceholderWrapper();
            modifier.PreCacheModify("example/url").ShouldEqual("<CASSETTE_URL_ROOT>example/url</CASSETTE_URL_ROOT>");
        }
    }
}