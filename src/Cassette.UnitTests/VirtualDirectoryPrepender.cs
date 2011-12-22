using Should;
using Xunit;

namespace Cassette
{
    public class VirtualDirectoryPrepender_Tests
    {
        [Fact]
        public void GivenVirtualDirectoryIsJustSlash_WhenModifyUrl_ThenSlashPrependsUrl()
        {
            var modifier = new VirtualDirectoryPrepender("/");
            var url = modifier.Modify("/test");
            url.ShouldEqual("/test");
        }

        [Fact]
        public void GivenVirtualDirectoryIsJustSlash_WhenModifyRelativeUrl_ThenSlashPrependsUrl()
        {
            var modifier = new VirtualDirectoryPrepender("/");
            var url = modifier.Modify("test");
            url.ShouldEqual("/test");
        }

        [Fact]
        public void GivenVirtualDirectoryIsSlashAndDirectoryName_WhenModifyUrl_ThenSlashDirectoryNamePrependsUrl()
        {
            var modifier = new VirtualDirectoryPrepender("/base");
            var url = modifier.Modify("/test");
            url.ShouldEqual("/base/test");
        }
    }
}
