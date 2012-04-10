using System.Collections.Generic;
using System.Linq;

namespace Cassette.Configuration
{
    public class ServiceRegistry : IServiceRegistry
    {
        public virtual IEnumerable<TypeRegistration> TypeRegistrations
        {
            get { return Enumerable.Empty<TypeRegistration>(); }
        }

        public virtual IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations
        {
            get { return Enumerable.Empty<CollectionTypeRegistration>(); }
        }

        public virtual IEnumerable<InstanceRegistration> InstanceRegistrations
        {
            get { return Enumerable.Empty<InstanceRegistration>(); }
        }
    }
}