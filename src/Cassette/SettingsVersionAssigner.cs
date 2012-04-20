using Cassette.Configuration;

namespace Cassette
{
    [ConfigurationOrder(1)]
    class SettingsVersionAssigner : IConfiguration<CassetteSettings>
    {
        readonly AssemblyScanner assemblyScanner;

        public SettingsVersionAssigner(AssemblyScanner assemblyScanner)
        {
            this.assemblyScanner = assemblyScanner;
        }

        public void Configure(CassetteSettings settings)
        {
            settings.Version = assemblyScanner.HashAssemblies();
        }
    }
}