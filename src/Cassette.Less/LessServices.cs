using Cassette.TinyIoC;

namespace Cassette.Stylesheets
{
    [ConfigurationOrder(20)]
    public class LessServices : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            // Explicitly call the default LessCompiler constructor.
            // Otherwise TinyIoC will try to use the overload that requires a configuration object.
            // That then causes a stackoverflow.
            container
                .Register<ILessCompiler>((c, n) => new LessCompiler());
        }
    }
}