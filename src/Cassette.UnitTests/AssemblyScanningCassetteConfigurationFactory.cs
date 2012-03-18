#if !NET35
using System;
using System.Linq;
using System.Reflection;
using Cassette.Configuration;
using Should;
using Xunit;

namespace Cassette
{
    public class AssemblyScanningCassetteConfigurationFactory_Tests
    {
        [Fact]
        public void CreateCassetteConfigurationsPutsCassetteConfigurationsBeforeApplicationConfigurations()
        {
            var assembly = new FakeAssembly();
            var factory = new AssemblyScanningCassetteConfigurationFactory(new[] { assembly });

            var configurations = factory.CreateCassetteConfigurations();
            
            var createdTypes = configurations.Select(c => c.GetType());
            createdTypes.ShouldEqual(new[]
            {
                typeof(Scripts.CoffeeScriptConfiguration),
                typeof(StubApplication.CassetteConfiguration)
            });
        }

        class FakeAssembly : Assembly
        {
            public override Type[] GetTypes()
            {
                return new[]
            {
                typeof(StubApplication.CassetteConfiguration),
                typeof(Scripts.CoffeeScriptConfiguration)
            };
            }
        }
    }

}

namespace StubApplication
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
#endif