using System.Web;
using Should;
using Xunit;

namespace Knapsack.Configuration
{
    public abstract class Given_KnapsackSection
    {
        protected KnapsackSection section;

        public Given_KnapsackSection(ModuleMode moduleMode)
        {
            section = new KnapsackSection { ModuleMode = moduleMode };
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

    public class Given_KnapsackSection_where_ModuleMode_is_On : Given_KnapsackSection
    {
        public Given_KnapsackSection_where_ModuleMode_is_On()
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

    public class Given_KnapsackSection_where_ModuleMode_is_Off : Given_KnapsackSection
    {
        public Given_KnapsackSection_where_ModuleMode_is_Off()
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

    public class Given_KnapsackSection_where_ModuleMode_is_OffInDebug : Given_KnapsackSection
    {
        public Given_KnapsackSection_where_ModuleMode_is_OffInDebug()
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

    public class KnapsackSection_defaults
    {
        [Fact]
        public void BufferHtmlOutput_is_true()
        {
            new KnapsackSection().BufferHtmlOutput.ShouldBeTrue();
        }

        [Fact]
        public void ModuleMode_is_OffInDebug()
        {
            new KnapsackSection().ModuleMode.ShouldEqual(ModuleMode.OffInDebug);
        }
    }
}
