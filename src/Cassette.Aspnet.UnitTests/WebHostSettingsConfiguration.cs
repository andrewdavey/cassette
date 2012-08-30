using System;
using System.IO;
using Cassette.IO;
using Should;
using Xunit;

namespace Cassette.Aspnet
{
    public class WebHostSettingsConfiguration_Tests
    {
        [Fact]
        public void UseIsolatedStorageWhenCachedDirectoryNotConfigured()
        {
            var section = new CassetteConfigurationSection();
            var configuration = new TestableWebHostSettingsConfiguration("/", section);
            var settings = new CassetteSettings();
            configuration.Configure(settings);
            settings.CacheDirectory.ShouldBeType<IsolatedStorageDirectory>();
        }

        [Fact]
        public void UseFileSystemDirectoryWhenCacheDirectoryIsConfigured()
        {
            var section = new CassetteConfigurationSection
            {
                CacheDirectory = Environment.CurrentDirectory
            };
            var configuration = new TestableWebHostSettingsConfiguration("/", section);
            var settings = new CassetteSettings();
            configuration.Configure(settings);
            settings.CacheDirectory.ShouldBeType<FileSystemDirectory>();
        }

        [Fact]
        public void UseAbsoluteFileSystemDirectoryWhenCacheDirectoryIsConfiguredAsRelativePath()
        {
            var section = new CassetteConfigurationSection
            {
                CacheDirectory = "test"
            };
            var configuration = new TestableWebHostSettingsConfiguration("/", section);
            var settings = new CassetteSettings();
            configuration.Configure(settings);

            var directory = settings.CacheDirectory.ShouldBeType<FileSystemDirectory>();
            directory.FullSystemPath.ShouldEqual(Path.Combine(Environment.CurrentDirectory, "test").Replace('\\', '/'));
        }

        class TestableWebHostSettingsConfiguration : WebHostSettingsConfiguration
        {
            readonly CassetteConfigurationSection section;

            public TestableWebHostSettingsConfiguration(string virtualDirectory, CassetteConfigurationSection section)
                : base(virtualDirectory)
            {
                this.section = section;
            }

            protected override string AppDomainAppPath
            {
                get
                {
                    return Environment.CurrentDirectory;
                }
            }

            protected override CassetteConfigurationSection GetConfigurationSection()
            {
                return section;
            }
        }
    }
}