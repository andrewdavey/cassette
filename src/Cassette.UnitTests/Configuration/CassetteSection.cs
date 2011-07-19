using System.Web;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public abstract class Given_CassetteSection
    {
        protected CassetteSection section;

        public Given_CassetteSection(ModuleMode moduleMode)
        {
            section = new CassetteSection { ModuleMode = moduleMode };
        }

        protected HttpContextBase DebugMode()
        {
            return new FakeHttpContext(true);
        }

        protected HttpContextBase ReleaseMode()
        {
            return new FakeHttpContext(false);
        }

        class FakeHttpContext : HttpContextBase
        {
            public FakeHttpContext(bool debugging)
            {
                this.debugging = debugging;
            }

            bool debugging;

            public override bool IsDebuggingEnabled
            {
                get { return debugging; }
            }
        }
    }

    public class Given_CassetteSection_where_ModuleMode_is_On : Given_CassetteSection
    {
        public Given_CassetteSection_where_ModuleMode_is_On()
            : base(ModuleMode.On) { }

        [Fact]
        public void ShouldUseModules_returns_true()
        {
            section.ShouldUseModules(null).ShouldBeTrue();
        }

        [Fact]
        public void When_web_app_in_debug_mode_ShouldUseModules_returns_true()
        {
            section.ShouldUseModules(DebugMode()).ShouldBeTrue();
        }
    }

    public class Given_CassetteSection_where_ModuleMode_is_Off : Given_CassetteSection
    {
        public Given_CassetteSection_where_ModuleMode_is_Off()
            : base(ModuleMode.Off) { }

        [Fact]
        public void ShouldUseModules_returns_false()
        {
            section.ShouldUseModules(null).ShouldBeFalse();
        }

        [Fact]
        public void When_web_app_not_in_debug_mode_ShouldUseModules_returns_false()
        {
            section.ShouldUseModules(ReleaseMode()).ShouldBeFalse();
        }
    }

    public class Given_CassetteSection_where_ModuleMode_is_OffInDebug : Given_CassetteSection
    {
        public Given_CassetteSection_where_ModuleMode_is_OffInDebug()
            : base(ModuleMode.OffInDebug) { }

        [Fact]
        public void When_web_app_in_release_mode_ShouldUseModules_returns_true()
        {
            section.ShouldUseModules(ReleaseMode()).ShouldBeTrue();
        }

        [Fact]
        public void When_web_app_not_in_debug_mode_ShouldUseModules_returns_false()
        {
            section.ShouldUseModules(DebugMode()).ShouldBeFalse();
        }
    }

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
