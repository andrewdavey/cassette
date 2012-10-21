using System.Collections.Generic;

namespace Cassette.RequireJS
{
    public class Shim
    {
        /// <summary>
        /// The JavaScript expression that will be used to return the module's value.
        /// For example, this could be the name of a global variable.
        /// </summary>
        public string ModuleReturnExpression { get; set; }

        public IEnumerable<string> Dependencies { get; set; }

        public string Alias { get; set; }
    }
}