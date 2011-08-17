using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette
{
    public class ModuleSourceResult<T>
        where T : Module
    {
        public ModuleSourceResult(IEnumerable<T> modules, DateTime lastWriteTimeMax)
        {
            this.modules = modules;
            this.lastWriteTimeMax = lastWriteTimeMax;
        }

        readonly IEnumerable<T> modules;
        readonly DateTime lastWriteTimeMax;

        public IEnumerable<T> Modules
        {
            get { return modules; }
        }

        public DateTime LastWriteTimeMax
        {
            get { return lastWriteTimeMax; }
        }

        public ModuleSourceResult<T> Merge(ModuleSourceResult<T> result)
        {
            var max = (lastWriteTimeMax > result.lastWriteTimeMax) 
                ? lastWriteTimeMax
                : result.lastWriteTimeMax;

            return new ModuleSourceResult<T>(
                modules.Concat(result.modules),
                max
            );
        }
    }
}