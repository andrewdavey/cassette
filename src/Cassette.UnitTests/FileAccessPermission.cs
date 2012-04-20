using Should;
using Xunit;
namespace Cassette
{
    public class FileAccessPermission_Tests
    {
        readonly FileAccessPermission permission = new FileAccessPermission();

        [Fact]
        public void GivenAllowAccessToPath_ThenCanAccessPathReturnsTrue()
        {
            permission.AllowAccess("~/test.png");
            permission.CanAccess("~/test.png").ShouldBeTrue();
        }

        [Fact]
        public void GivenNoPathsAllowed_ThenCanAccessPathReturnsFalse()
        {
            permission.CanAccess("~/test.png").ShouldBeFalse();
        }
    }

    public class FileAccessPermission
    {
        public void AllowAccess(string path)
        {
            
        }

        public bool CanAccess(string path)
        {
            return true;
        }
    }
}