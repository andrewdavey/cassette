using System;

namespace Cassette.Configuration
{
    /// <summary>
    /// Services used by Cassette.
    /// </summary>
    public class CassetteServices
    {
        public Func<IUrlModifier> CreateUrlModifier { get; set; }
    }
}