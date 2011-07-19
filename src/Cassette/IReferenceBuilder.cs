using System;
using System.Collections.Generic;

namespace Cassette
{
    public interface IReferenceBuilder
    {
        void AddReference(string filename);
        void AddExternalReference(string externalUrl, string location);
        IEnumerable<Module> GetRequiredModules();
    }
}
