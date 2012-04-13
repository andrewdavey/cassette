using Cassette.Interop;
using TinyIoC;

namespace Cassette.Scripts
{
#if NET35
    public class CoffeeScriptServices : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            container.Register(typeof(ICoffeeScriptCompiler), typeof(JurassicCoffeeScriptCompiler)).AsSingleton();
        }
    }
#else
    public class CoffeeScriptServices : IConfiguration<TinyIoCContainer>
    {
        public void Configure(TinyIoCContainer container)
        {
            container.Register(typeof(IECoffeeScriptCompilationQueue)).AsSingleton();
            container.Register(typeof(IECoffeeScriptCompiler));
            container.Register(typeof(JurassicCoffeeScriptCompiler)).AsSingleton();
            container.Register(typeof(ICoffeeScriptCompiler), (c, p) => GetCompiler(c));
        }

        ICoffeeScriptCompiler GetCompiler(TinyIoCContainer container)
        {
            try
            {
                // Attempt to create the COM-based IE JavaScript engine.
                using (new IEJavaScriptEngine()){}
                return container.Resolve<IECoffeeScriptCompiler>();
            }
            catch
            {
                // Failure could be due to medium trust, IE not installed, etc...
                // Fallback to using Jurassic instead.
                return container.Resolve<JurassicCoffeeScriptCompiler>();
            }
        }
    }
#endif
}