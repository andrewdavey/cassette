using System;
using System.Collections.Generic;

namespace Cassette.RequireJS
{
    public class RequireJsSettings
    {
        public RequireJsSettings(IEnumerable<IConfiguration<RequireJsSettings>> configurations)
        {
            foreach (var configuration in configurations)
            {
                configuration.Configure(this);
            }

            if (string.IsNullOrEmpty(RequireJsPath))
            {
                throw new InvalidOperationException("RequireJsPath has not been assigned by any configuration class. Ensure there is a public class that implements IConfiguration<RequireJsSettings>.");
            }
        }

        public string RequireJsPath { get; set; }
    }
}