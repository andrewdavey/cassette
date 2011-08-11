using System;
using System.Collections.Generic;

namespace Cassette
{
    public interface IReferenceBuilder<T>
        where T : Module
    {
        void AddReference(string path);
        IEnumerable<T> GetModules();
    }
}