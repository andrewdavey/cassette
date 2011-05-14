using System;
using System.Collections.Generic;

namespace Knapsack
{
    public interface IReferenceBuilder
    {
        void AddReference(string filename);
        IEnumerable<Module> GetRequiredModules();
    }
}
