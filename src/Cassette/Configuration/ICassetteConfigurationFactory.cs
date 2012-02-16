using System.Collections.Generic;

namespace Cassette.Configuration
{
    interface ICassetteConfigurationFactory
    {
        IEnumerable<ICassetteConfiguration> CreateCassetteConfigurations();
    }
}