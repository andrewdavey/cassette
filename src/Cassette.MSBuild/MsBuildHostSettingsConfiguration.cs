using Cassette.IO;

namespace Cassette.MSBuild
{
    class MsBuildHostSettingsConfiguration : IConfiguration<CassetteSettings>
    {
        readonly string sourceDirectory;
        readonly string outputDirectory;

        public MsBuildHostSettingsConfiguration(string sourceDirectory, string outputDirectory)
        {
            this.sourceDirectory = sourceDirectory;
            this.outputDirectory = outputDirectory;
        }

        public void Configure(CassetteSettings settings)
        {
            settings.SourceDirectory = new FileSystemDirectory(sourceDirectory);
            settings.CacheDirectory = new FileSystemDirectory(outputDirectory);
        }
    }
}