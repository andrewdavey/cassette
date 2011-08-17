namespace Cassette
{
    /// <summary>
    /// Configures a CassetteApplication by defining the asset modules.
    /// </summary>
    public interface ICassetteConfiguration
    {
        void Configure(ModuleConfiguration moduleConfiguration);
    }
}