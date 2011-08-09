namespace Cassette
{
    public static class CassetteApplicationExtensions
    {
        public static FileSystemModuleConfiguration<T> HasModules<T>(this ICassetteApplication application)
            where T : Module
        {
            var config = new FileSystemModuleConfiguration<T>(application);
            application.AddModuleContainerFactory(config);
            return config;
        }
    }
}