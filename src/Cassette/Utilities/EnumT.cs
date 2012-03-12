﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cassette.Utilities
{
    public static class Enum<T> where T : struct
    {
        private static readonly IEnumerable<T> All = Enum.GetValues(typeof(T)).Cast<T>();
        private static readonly Dictionary<string, T> InsensitiveNames = All.ToDictionary(k => Enum.GetName(typeof(T), k).ToLowerInvariant());
        private static readonly Dictionary<string, T> SensitiveNames = All.ToDictionary(k => Enum.GetName(typeof(T), k));
        private static readonly Dictionary<int, T> Values = All.ToDictionary(k => Convert.ToInt32(k));
        private static readonly Dictionary<T, string> Names = All.ToDictionary(k => k, v => v.ToString());

        public static bool IsDefined(T value)
        {
            return Names.Keys.Contains(value);
        }

        public static bool IsDefined(string value)
        {
            return SensitiveNames.Keys.Contains(value);
        }

        public static bool IsDefined(int value)
        {
            return Values.Keys.Contains(value);
        }

        public static IEnumerable<T> GetValues()
        {
            return All;
        }

        public static string[] GetNames()
        {
            return Names.Values.ToArray();
        }

        public static string GetName(T value)
        {
            string name;
            Names.TryGetValue(value, out name);
            return name;
        }

        public static T Parse(string value)
        {
            T parsed = default(T);
            if (!SensitiveNames.TryGetValue(value, out parsed))
                throw new ArgumentException("Value is not one of the named constants defined for the enumeration", "value");
            return parsed;
        }

        public static T Parse(string value, bool ignoreCase)
        {
            if (!ignoreCase)
                return Parse(value);

            T parsed = default(T);
            if (!InsensitiveNames.TryGetValue(value.ToLowerInvariant(), out parsed))
                throw new ArgumentException("Value is not one of the named constants defined for the enumeration", "value");
            return parsed;
        }

        public static bool TryParse(string value, out T returnValue)
        {
            return SensitiveNames.TryGetValue(value, out returnValue);
        }

        public static bool TryParse(string value, bool ignoreCase, out T returnValue)
        {
            if (!ignoreCase)
                return TryParse(value, out returnValue);

            return InsensitiveNames.TryGetValue(value.ToLowerInvariant(), out returnValue);
        }

        public static T? ParseOrNull(string value)
        {
            if (String.IsNullOrEmpty(value))
                return null;

            T foundValue;
            if (InsensitiveNames.TryGetValue(value.ToLowerInvariant(), out foundValue))
                return foundValue;

            return null;
        }

        public static T? CastOrNull(int value)
        {
            T foundValue;
            if (Values.TryGetValue(value, out foundValue))
                return foundValue;

            return null;
        }
    }

    public static class EnumExtensions
    {
        /// <summary>
        /// Check to see if a flags enumeration has a specific flag set.
        /// </summary>
        /// <param name="variable">Flags enumeration to check</param>
        /// <param name="value">Flag to check for</param>
        /// <returns></returns>
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
                    "Enumeration type mismatch.  The flag is of type '{0}', was expecting '{1}'.",
                    value.GetType(), variable.GetType()));
            }

            ulong num = Convert.ToUInt64(value);
            return ((Convert.ToUInt64(variable) & num) == num);

        }

    }
}
