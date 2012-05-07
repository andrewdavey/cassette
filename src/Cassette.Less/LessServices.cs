using Cassette.TinyIoC;

namespace Cassette.Stylesheets
{
    [ConfigurationOrder(20)]
    public class LessServices : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            container.Register<ILessCompiler, LessCompiler>().AsMultiInstance();
        }
    }
}