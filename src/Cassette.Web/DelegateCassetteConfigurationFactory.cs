using System;
using System.Collections.Generic;
using Cassette.Configuration;

namespace Cassette.Web
{
    class DelegateCassetteConfigurationFactory : ICassetteConfigurationFactory
    {
        readonly Func<IEnumerable<ICassetteConfiguration>> getCassetteConfigurations;

        public DelegateCassetteConfigurationFactory(Func<IEnumerable<ICassetteConfiguration>> getCassetteConfigurations)
        {
            this.getCassetteConfigurations = getCassetteConfigurations;
        }

        public IEnumerable<ICassetteConfiguration> CreateCassetteConfigurations()
        {
            return getCassetteConfigurations();
        }
    }
}