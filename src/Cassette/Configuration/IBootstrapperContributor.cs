using System.Collections.Generic;

namespace Cassette.Configuration
{
    public interface IBootstrapperContributor
    {
        IEnumerable<TypeRegistration> TypeRegistrations { get; }
        IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations { get; }
        IEnumerable<InstanceRegistration> InstanceRegistrations { get; } 
    }
}