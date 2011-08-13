using System;
using System.Collections.Generic;

namespace Cassette
{
    public interface IReferenceBuilder<T>
        where T : Module
    {
        void AddReference(string path, string location);
        IEnumerable<T> GetModules(string location);
    }
}