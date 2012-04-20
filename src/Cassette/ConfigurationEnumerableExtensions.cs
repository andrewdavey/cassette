using System.Collections.Generic;
using System.Linq;

namespace Cassette
{
    public static class ConfigurationEnumerableExtensions
    {
        public static IEnumerable<IConfiguration<T>> OrderByConfigurationOrderAttribute<T>(this IEnumerable<IConfiguration<T>> configurations)
        {
            return configurations.OrderBy(c => ConfigurationOrderAttribute.GetOrder(c.GetType()));
        }
    }
}