using System.Collections.Generic;

namespace Cassette.UI
{
    public interface IReferenceBuilder<T>
        where T : Module
    {
        void AddReference(string path, string location);
        void AddReference(Module module, string location);
        IEnumerable<Module> GetModules(string location);
    }
}