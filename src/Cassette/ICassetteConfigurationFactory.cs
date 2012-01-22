using System.Collections.Generic;
using Cassette.Configuration;

namespace Cassette
{
    internal interface ICassetteConfigurationFactory
    {
        IEnumerable<ICassetteConfiguration> CreateCassetteConfigurations();
    }
}