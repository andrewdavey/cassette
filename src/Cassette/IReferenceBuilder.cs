using System.Collections.Generic;

namespace Cassette
{
    public interface IReferenceBuilder
    {
        void AddReference(string path, string location);
        void AddReference(Module module, string location);
        IEnumerable<Module> GetModules(string location);
    }
}