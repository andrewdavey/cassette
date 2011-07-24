using System.Web;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class CassetteSection_defaults
    {
        [Fact]
        public void BufferHtmlOutput_is_true()
        {
            new CassetteSection().BufferHtmlOutput.ShouldBeTrue();
        }

        [Fact]
        public void ModuleMode_is_OffInDebug()
        {
            new CassetteSection().ModuleMode.ShouldEqual(ModuleMode.OffInDebug);
        }
    }
}
