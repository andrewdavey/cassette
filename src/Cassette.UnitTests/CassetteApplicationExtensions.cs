using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class CassetteApplicationExtensions_Tests
    {
        [Fact]
        public void HasModulesReturnsFileSystemModuleConfiguration()
        {
            var app = new Mock<ICassetteApplication>();
            var config = app.Object.HasModules<Module>();
            config.ShouldBeType<FileSystemModuleConfiguration<Module>>();
        }

        [Fact]
        public void HasModulesAddsConfigurationToTheApplication()
        {
            var app = new Mock<ICassetteApplication>();
            var config = app.Object.HasModules<Module>();
            app.Verify(a => a.AddModuleContainerFactory<Module>(config));
        }
    }
}
