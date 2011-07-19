using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace Cassette
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
        Added,
        Deleted
    }
}
