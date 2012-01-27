using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;

namespace Cassette
{
    /// <summary>
    /// A collection of AttributeName/AttributeValue pairs.
    /// </summary>
    public class HtmlAttributeDictionary : IEnumerable<KeyValuePair<string, string>>
    {
        readonly Dictionary<string, string> attributeStorage = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Add an object to the <see cref="HtmlAttributeDictionary"/> using the Property Names as the Attribute Name
        /// and the Property Values as the Attribute Values.
        /// </summary>
        /// <param name="values">The object containing the attributes to add.</param>
        /// <returns>This dictionary.</returns>
        public HtmlAttributeDictionary Add(object values)
        {
            if (values == null)
            {
                return this;
            }
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(values);

            foreach (PropertyDescriptor property in properties)
            {
                object propertyValue = property.GetValue(values);
                this.Add(property.Name, propertyValue);
            }

            return this;
        }

        /// <summary>
        /// Add an attribute to the <see cref="HtmlAttributeDictionary"/> that has a blank value.
        /// </summary>
        /// <param name="name">The attribute name to add.</param>
        /// <returns>This dictionary.</returns>
        /// <exception cref="ArgumentException">Thrown if name is invalid.</exception>
        public HtmlAttributeDictionary Add(string name)
        {
            return Add(name, null);
        }

        /// <summary>
        /// Add an attribute name and attribute value to the <see cref="HtmlAttributeDictionary"/>.
        /// </summary>
        /// <param name="name">The attribute name to add.</param>
        /// <param name="value">The attribute value to add.</param>
        /// <returns>This dictionary.</returns>
        /// <exception cref="ArgumentException">Thrown if name is invalid.</exception>
        public HtmlAttributeDictionary Add(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.", "name");

            attributeStorage.Add(SanitizeName(name), SanitizeValue(value));

            return this;
        }

        /// <summary>
        /// Determines whether the <see cref="HtmlAttributeDictionary"/> containes the specified attribute name.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <returns>True if found.</returns>
        /// <exception cref="ArgumentException">Thrown if name is invalid.</exception>
        public bool ContainsAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.", "name");

            return attributeStorage.ContainsKey(SanitizeName(name));
        }

        /// <summary>
        /// Removes the attribute with the specified name from the <see cref="HtmlAttributeDictionary"/>.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <returns>True if an attribute was removed.</returns>
        /// <exception cref="ArgumentException">Thrown if name is invalid.</exception>
        public bool Remove(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.", "name");

            return attributeStorage.Remove(SanitizeName(name));
        }

        public string this[string name]
        {
            get
            {
                return attributeStorage[SanitizeName(name)];
            }
            set
            {
                attributeStorage[SanitizeName(name)] = SanitizeValue(value);
            }
        }

        /// <summary>
        /// Gets the number of attribute name/value pairs in the <see cref="HtmlAttributeDictionary"/>.
        /// </summary>
        public int Count                                        
        {
            get { return attributeStorage.Count; }
        }

        /// <summary>
        /// Generates an Html attributes string (with a leading space).
        /// The attribute name will be the dictionary key and the attribute value will be the dictionary value.
        /// </summary>
        internal string CombinedAttributes
        {
            get
            {
                // Short-circuit on empty dictionary
                if (attributeStorage.Count == 0)
                    return string.Empty;

                // Construct attribute Line from dictionary entries

                StringBuilder sb = new StringBuilder(256);

                foreach (KeyValuePair<string, string> attribute in attributeStorage)
                {
                    if (attribute.Value == null)
                        sb.AppendFormat(" {0}", attribute.Key);
                    else
                        sb.AppendFormat(" {0}=\"{1}\"", attribute.Key, attribute.Value);
                }
                return sb.ToString();
            }
        }

        #region IEnumerable<KeyValuePair<string, string>> Implementation

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="HtmlAttributeDictionary"/>.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return attributeStorage.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="HtmlAttributeDictionary"/>.
        /// </summary>
        /// <returns>The enumerator.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return attributeStorage.GetEnumerator();
        }

        #endregion

        #region Private Helpers

        private static string SanitizeName(string name)
        {
            // XHTML requires lowercase attribute names
            // W3C recommends lowercase attribute names in the HTML 4 recommendation.
            return name.Trim().Replace('-', '_').ToLowerInvariant();
        }

        private static string SanitizeValue(object value)
        {
            if (value == null )
                return null;

            // Values should be supplied already escaped
            // But if a quote makes its way in, be a good citizen and escape it.
            return value.ToString().Replace("\"", "&quot;");
        }

        #endregion

    }
}
