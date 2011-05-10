using System;
using System.Collections.Generic;

namespace Knapsack
{
    public interface IReferenceBuilder
    {
        void AddReference(string scriptPath);
        IEnumerable<Module> GetRequiredModules();
    }
}
