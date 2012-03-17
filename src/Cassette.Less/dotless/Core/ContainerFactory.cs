namespace dotless.Core
{
    using Cache;
    using configuration;
    using Input;
    using Loggers;
    using Microsoft.Practices.ServiceLocation;
    using Pandora;
    using Pandora.Fluent;
    using Parameters;
    using Stylizers;

    class ContainerFactory
    {
        private PandoraContainer Container { get; set; }

        public IServiceLocator GetContainer(DotlessConfiguration configuration)
        {
            Container = new PandoraContainer();

            Container.Register(pandora => RegisterServices(pandora, configuration));

            return new CommonServiceLocatorAdapter(Container);
        }

        private void RegisterServices(FluentRegistration pandora, DotlessConfiguration configuration)
        {
            OverrideServices(pandora, configuration);

            RegisterLocalServices(pandora);

            RegisterCoreServices(pandora, configuration);
        }

        private void OverrideServices(FluentRegistration pandora, DotlessConfiguration configuration)
        {
            if (configuration.Logger != null)
                pandora.Service<ILogger>().Implementor(configuration.Logger);
        }

        private void RegisterLocalServices(FluentRegistration pandora)
        {
            pandora.Service<ICache>().Implementor<InMemoryCache>();
            pandora.Service<IParameterSource>().Implementor<ConsoleArgumentParameterSource>();
            pandora.Service<ILogger>().Implementor<ConsoleLogger>().Parameters("level").Set("error-level");
            pandora.Service<IPathResolver>().Implementor<RelativePathResolver>();
        }

        private void RegisterCoreServices(FluentRegistration pandora, DotlessConfiguration configuration)
        {
            pandora.Service<LogLevel>("error-level").Instance(configuration.LogLevel);
            pandora.Service<IStylizer>().Implementor<PlainStylizer>();

            pandora.Service<Parser.Parser>().Implementor<Parser.Parser>().Parameters("optimization").Set("default-optimization").Lifestyle.Transient();
            pandora.Service<int>("default-optimization").Instance(configuration.Optimization);

            pandora.Service<ILessEngine>().Implementor<ParameterDecorator>().Lifestyle.Transient();

            if (configuration.CacheEnabled)
                pandora.Service<ILessEngine>().Implementor<CacheDecorator>().Lifestyle.Transient();

            pandora.Service<ILessEngine>().Implementor<LessEngine>().Parameters("compress").Set("minify-output").Lifestyle.Transient();
            pandora.Service<bool>("minify-output").Instance(configuration.MinifyOutput);

            pandora.Service<IFileReader>().Implementor(configuration.LessSource);
        }
    }
}