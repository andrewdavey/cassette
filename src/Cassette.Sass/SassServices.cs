using TinyIoC;

namespace Cassette.Stylesheets
{
    public class SassServices : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            container.Register<ISassCompiler, SassCompiler>();
        }
    }
}