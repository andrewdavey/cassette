using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knapsack
{
    public class ModuleDifference
    {
        readonly Module module;
        readonly ModuleDifferenceType type;

        public ModuleDifference(Module module, ModuleDifferenceType type)
        {
            this.module = module;
            this.type = type;
        }

        public Module Module
        {
            get { return module; }
        }

        public ModuleDifferenceType Type
        {
            get { return type; }
        }
    }

    public enum ModuleDifferenceType
    {
        Changed,
        Added,
        Deleted
    }
}
