using Cassette.BundleProcessing;
using Cassette.TinyIoC;
using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HandlebarsServices_Tests
    {
        readonly TinyIoCContainer container;
        readonly HandlebarsServices services;

        public HandlebarsServices_Tests()
        {
            container = new TinyIoCContainer();
            container.Register(Mock.Of<IUrlGenerator>());
            container.Register<IHtmlTemplateIdStrategy>(new HtmlTemplateIdBuilder());

            services = new HandlebarsServices();
        }

        [Fact]
        public void RegistersHandlebarsPipeline()
        {
            services.Configure(container);

            var pipeline = container.Resolve<IBundlePipeline<HtmlTemplateBundle>>();
            pipeline.ShouldBeType<HandlebarsPipeline>();
        }

        [Fact]
        public void AppliesHandlebarsSettingsConfigurations()
        {
            var configuration1 = new Mock<IConfiguration<HandlebarsSettings>>(); 
            var configuration2 = new Mock<IConfiguration<HandlebarsSettings>>();
            container.Register(configuration1.Object, "a");
            container.Register(configuration2.Object, "b");

            services.Configure(container);

            var settings = container.Resolve<HandlebarsSettings>();
            configuration1.Verify(c => c.Configure(settings));
            configuration2.Verify(c => c.Configure(settings));
        }
    }
}