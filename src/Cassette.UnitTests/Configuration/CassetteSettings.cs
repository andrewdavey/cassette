using Moq;
using Xunit;

namespace Cassette.Configuration
{
    public class CassetteSettings_Tests
    {
        [Fact]
        public void ConstructorAppliesConfigurations()
        {
            var config1 = new Mock<IConfiguration<CassetteSettings>>();
            var config2 = new Mock<IConfiguration<CassetteSettings>>();
            var settings = new CassetteSettings(new[] { config1.Object, config2.Object });
            config1.Verify(c => c.Configure(settings));
            config2.Verify(c => c.Configure(settings));
        }
    }
}