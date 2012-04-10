using System.Collections.Generic;

namespace Cassette.Configuration
{
    public class ServiceRegistryBase : IServiceRegistry
    {
        public virtual IEnumerable<TypeRegistration> TypeRegistrations
        {
            get { yield break; }
        }

        public virtual IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations
        {
            get { yield break; }
        }

        public virtual IEnumerable<InstanceRegistration> InstanceRegistrations
        {
            get { yield break; }
        }
    }
}