#if NET35
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette.Utilities
{
    static class Enum<T> where T : struct
    {
        private static readonly IEnumerable<T> All = Enum.GetValues(typeof(T)).Cast<T>();
        private static readonly Dictionary<string, T> SensitiveNames = All.ToDictionary(k => Enum.GetName(typeof(T), k));

        public static bool TryParse(string value, out T returnValue)
        {
            return SensitiveNames.TryGetValue(value, out returnValue);
        }
    }

    static class EnumExtensions
    {
        /// <summary>
        /// Check to see if a flags enumeration has a specific flag set.
        /// </summary>
        /// <remarks>Code based on http://stackoverflow.com/a/4108907 </remarks>
        /// <param name="variable">Flags enumeration to check</param>
        /// <param name="value">Flag to check for</param>
        public static bool HasFlag(this Enum variable, Enum value)
        {
            if (variable == null)
                return false;

            if (value == null)
                throw new ArgumentNullException("value");

            // Not as good as the .NET 4 version of this function, but should be good enough
            if (!Enum.IsDefined(variable.GetType(), value))
            {
                throw new ArgumentException(string.Format(
                    "Enumeration type mismatch. The flag is of type '{0}', was expecting '{1}'.",
                    value.GetType(), variable.GetType()));
            }

            var num = Convert.ToUInt64(value);
            return (Convert.ToUInt64(variable) & num) == num;
        }
    }
}
#endif