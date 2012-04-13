using System;
using System.Linq;

namespace Cassette
{
    /// <summary>
    /// Defines a value used to order the execution of configurations.
    /// </summary>
    public class ConfigurationOrderAttribute : Attribute
    {
        public int Order { get; private set; }

        public ConfigurationOrderAttribute(int order)
        {
            Order = order;
        }

        internal static int GetOrder(Type type)
        {
            var attribute = type
                .GetCustomAttributes(typeof(ConfigurationOrderAttribute), false)
                .OfType<ConfigurationOrderAttribute>()
                .FirstOrDefault();

            return attribute != null 
                ? attribute.Order 
                : int.MaxValue; // Types with no order attribute should run last.
        }
    }
}