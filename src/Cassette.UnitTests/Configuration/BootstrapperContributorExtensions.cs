using System.Linq;
using Should;

namespace Cassette.Configuration
{
    static class BootstrapperContributorExtensions
    {
        public static T GetInstance<T>(this BootstrapperContributor contributor)
        {
            return (T)contributor.InstanceRegistrations.First(i => i.RegistrationType == typeof(T)).Instance;
        }

        public static void ShouldHaveTypeRegistration<TR,TI>(this BootstrapperContributor contributor)
        {
            contributor
                .TypeRegistrations
                .First(t => t.RegistrationType == typeof(TR))
                .ImplementationType
                .ShouldEqual(typeof(TI));
        }
    }
}