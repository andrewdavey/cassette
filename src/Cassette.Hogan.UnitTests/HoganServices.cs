using Cassette.BundleProcessing;
using Cassette.TinyIoC;
using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HoganServices_Tests
    {
        readonly TinyIoCContainer container;
        readonly HoganServices services;

        public HoganServices_Tests()
        {
            container = new TinyIoCContainer();
            container.Register(Mock.Of<IUrlGenerator>());

            services = new HoganServices();
        }

        [Fact]
        public void RegistersHoganPipeline()
        {
            services.Configure(container);

            var pipeline = container.Resolve<IBundlePipeline<HtmlTemplateBundle>>();
            pipeline.ShouldBeType<HoganPipeline>();
        }

        [Fact]
        public void AppliesHoganSettingsConfigurations()
        {
            var configuration1 = new Mock<IConfiguration<HoganSettings>>(); 
            var configuration2 = new Mock<IConfiguration<HoganSettings>>();
            container.Register(configuration1.Object, "a");
            container.Register(configuration2.Object, "b");

            services.Configure(container);

            var settings = container.Resolve<HoganSettings>();
            configuration1.Verify(c => c.Configure(settings));
            configuration2.Verify(c => c.Configure(settings));
        }
    }
}