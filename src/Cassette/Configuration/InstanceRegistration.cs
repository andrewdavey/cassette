using System;

namespace Cassette.Configuration
{
    public class InstanceRegistration
    {
        public Type RegistrationType { get; private set; }
        public object Instance { get; private set; }
        public string Name { get; private set; }

        public InstanceRegistration(Type registrationType, object instance) 
            : this(registrationType, instance, null)
        {
        }

        public InstanceRegistration(Type registrationType, object instance, string name)
        {
            if (registrationType == null)
            {
                throw new ArgumentNullException("registrationType");
            }
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            if (!registrationType.IsInstanceOfType(instance))
            {
                throw new ArgumentException(string.Format("Object must be an instance of {0} but was {1}.", registrationType.FullName, instance.GetType().FullName), "instance");
            }

            RegistrationType = registrationType;
            Instance = instance;
            Name = name;
        }
    }
}