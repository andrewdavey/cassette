using TinyIoC;

namespace Cassette.Stylesheets
{
    public class LessServices : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            container.Register<ILessCompiler, LessCompiler>().AsMultiInstance();
        }
    }
}