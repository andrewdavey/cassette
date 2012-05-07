using Cassette.TinyIoC;

namespace Cassette.Stylesheets
{
    [ConfigurationOrder(20)]
    public class SassServices : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            container.Register<ISassCompiler, SassCompiler>();
        }
    }
}