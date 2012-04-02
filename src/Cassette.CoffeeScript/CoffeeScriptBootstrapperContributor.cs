using System.Collections.Generic;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    public class CoffeeScriptBootstrapperContributor : BootstrapperContributor
    {
        public override IEnumerable<TypeRegistration> TypeRegistrations
        {
            get
            {
                yield return new TypeRegistration(typeof(ICoffeeScriptCompiler), typeof(JurassicCoffeeScriptCompiler));
            }
        }
    }
}