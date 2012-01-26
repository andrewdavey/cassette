using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;

namespace Cassette.Utilities
{
    static class DictionaryExtensions 
    {

        /// <summary>
        /// Create Name/Value dictionary pairs from an object.
        /// </summary>
        /// <param name="dictionary">The destination dictionary</param>
        /// <param name="values">The object</param>
        /// <param name="convertToLower">Convert property name to lower case if true</param>
        /// <returns>The destination dictionary</returns>
        internal static IDictionary<string, object> AddObjectProperties(this IDictionary<string, object> dictionary, object values, bool convertToLower = false)
        {
            if (values == null)
            {
                return dictionary;
        	}
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(values);

            foreach (PropertyDescriptor property in properties)
            {
                object propertyValue = property.GetValue(values);
                string propertyName = convertToLower ? property.Name.ToLowerInvariant() : property.Name;
                dictionary.Add(propertyName, propertyValue);
            }

            return dictionary;
        }

        /// <summary>
        /// Generate an Html attributes string (with a leading space).
        /// The attribute name will be the dictionary key and the attribute value will be the dictionary value.
        /// </summary>
        /// <param name="dictionary">The source dictionary</param>
        /// <returns>An attribute string with a leading space character</returns>
        internal static string HtmlAttributesString(this IDictionary<string, object> dictionary )
        {
            StringBuilder sb = new StringBuilder(256);

            foreach( KeyValuePair<string, object> attribute in dictionary)
            {
                sb.AppendFormat(" {0}=\"{1}\"", attribute.Key, attribute.Value.ToString().Replace("\"", "&quot;"));
            }
            return sb.ToString();
        }

    }
}
