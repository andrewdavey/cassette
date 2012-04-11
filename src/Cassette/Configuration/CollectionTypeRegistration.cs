using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette.Configuration
{
    public class CollectionTypeRegistration
    {
        public Type RegistrationType { get; private set; }
        public IEnumerable<Type> ImplementationTypes { get; private set; }

        public CollectionTypeRegistration(Type registrationType, IEnumerable<Type> implementationTypes)
        {
            if (registrationType == null)
            {
                throw new ArgumentNullException("registrationType");
            }
            if (implementationTypes == null)
            {
                throw new ArgumentNullException("implementationTypes");
            }
            implementationTypes = implementationTypes.ToArray();
            if (implementationTypes.Any(i => !registrationType.IsAssignableFrom(i)))
            {
                throw new ArgumentException("All implementationTypes must implement registrationType.", "implementationTypes");
            }

            RegistrationType = registrationType;
            ImplementationTypes = implementationTypes;
        }
    }
}