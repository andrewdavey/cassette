using System;
using Cassette.Configuration;
using Cassette.IO;

namespace Cassette.MSBuild
{
    class MsBuildHostSettingsConfiguration : IConfiguration<CassetteSettings>
    {
        public void Configure(CassetteSettings settings)
        {
            settings.SourceDirectory = new FileSystemDirectory(Environment.CurrentDirectory);            
        }
    }
}