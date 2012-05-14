using Cassette.IO;

namespace Cassette.MSBuild
{
    class MsBuildHostSettingsConfiguration : IConfiguration<CassetteSettings>
    {
        readonly string sourceDirectory;

        public MsBuildHostSettingsConfiguration(string sourceDirectory)
        {
            this.sourceDirectory = sourceDirectory;
        }

        public void Configure(CassetteSettings settings)
        {
            settings.SourceDirectory = new FileSystemDirectory(sourceDirectory);
        }
    }
}