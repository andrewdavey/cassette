using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class CassetteSettings_RequestRawFile_Tests
    {
        [Fact]
        public void ByDefaultDenyAllRawFileRequests()
        {
            var settings = new CassetteSettings();
            settings.CanRequestRawFile("~/file.png").ShouldBeFalse();
        }

        [Fact]
        public void GivenAllowRawFileRequestPredicate_ThenAllowRequest()
        {
            var settings = new CassetteSettings();
            settings.AllowRawFileRequest(path => path == "~/file.png");
            settings.CanRequestRawFile("~/file.png").ShouldBeTrue();
        }
    }
}