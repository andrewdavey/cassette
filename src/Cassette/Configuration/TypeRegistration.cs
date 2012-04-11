using System;

namespace Cassette.Configuration
{
    public class TypeRegistration
    {
        public Type RegistrationType { get; private set; }
        public Type ImplementationType { get; private set; }
        public string Name { get; private set; }

        public TypeRegistration(Type registrationType, Type implementationType)
            : this(registrationType, implementationType, "")
        {
        }

        public TypeRegistration(Type registrationType, Type implementationType, string name)
        {
            if (registrationType == null)
            {
                throw new ArgumentNullException("registrationType");
            }
            if (implementationType == null)
            {
                throw new ArgumentNullException("implementationType");
            }
            if (!registrationType.IsAssignableFrom(implementationType))
            {
                throw new ArgumentException(string.Format("implementationType {0} must implement registrationType {1}.", implementationType.FullName, registrationType.FullName), "implementationType");
            }

            RegistrationType = registrationType;
            ImplementationType = implementationType;
            Name = name;
        }
    }
}