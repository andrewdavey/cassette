using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Cassette.Utilities;

namespace Cassette
{
    /// <remarks>
    /// Contains a collection of html attribute name/value pairs.
    /// </remarks>
    public class HtmlAttributeDictionary : IEnumerable<KeyValuePair<string, string>>
    {
        readonly Dictionary<string, string> attributeStorage = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Add an object to the <see cref="HtmlAttributeDictionary"/> using the object property names as the attribute name
        /// and the property values as the attribute Values.
        /// 
        /// Underscores in the object property name will be converted to dashes.
        /// </summary>
        /// <example>
        /// <code>
        /// var attributes = new HtmlAttributeDictionary().Add( new { data_val_required = "contrived example", @class = "cssx" } );
        /// </code>
        /// </example>
        /// <param name="values">An object that contains the HTML attributes.
        /// The attributes are retrieved through reflection by examining the properties of the object.
        /// The object is typically created by using object initializer syntax.</param>
        /// <returns>This dictionary.</returns>
        public HtmlAttributeDictionary Add(object values)
        {
            if (values == null) return this;

            AddForEachPropertyOfObject(values);
            return this;
        }

        void AddForEachPropertyOfObject(object values)
        {
            var properties = TypeDescriptor.GetProperties(values);
            foreach (PropertyDescriptor property in properties)
            {
                var propertyValue = property.GetValue(values);
                var name = property.Name.Replace('_', '-');
                Add(name, propertyValue);
            }
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
        /// Add an attribute name and value to the <see cref="HtmlAttributeDictionary"/>.
        /// </summary>
        /// <param name="name">The attribute name to add.</param>
        /// <param name="value">The attribute value to add.</param>
        /// <returns>This dictionary.</returns>
        /// <exception cref="ArgumentException">Thrown if name is invalid.</exception>
        public HtmlAttributeDictionary Add(string name, object value)
        {
            RequireAttributeName(name);

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
            RequireAttributeName(name);

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
            RequireAttributeName(name);

            return attributeStorage.Remove(SanitizeName(name));
        }

        public string this[string name]
        {
            get
            {
                RequireAttributeName(name);
                return attributeStorage[SanitizeName(name)];
            }
            set
            {
                RequireAttributeName(name);
                attributeStorage[SanitizeName(name)] = SanitizeValue(value);
            }
        }

        void RequireAttributeName(string name)
        {
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentException("Attribute name is required.", "name");
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
                if (attributeStorage.Count == 0)
                    return string.Empty;

                return BuildAttributesString();
            }
        }

        string BuildAttributesString()
        {
            var builder = new StringBuilder(256);
            foreach (var attribute in attributeStorage)
            {
                AppendAttribute(builder, attribute);
            }
            return builder.ToString();
        }

        void AppendAttribute(StringBuilder builder, KeyValuePair<string, string> attribute)
        {
            if (attribute.Value == null)
            {
                builder.AppendFormat(" {0}", attribute.Key);
            }
            else
            {
                builder.AppendFormat(" {0}=\"{1}\"", attribute.Key, attribute.Value);
            }
        }

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

        string SanitizeName(string name)
        {
            // XHTML requires lowercase attribute names
            // W3C recommends lowercase attribute names in the HTML 4 recommendation.
            return name.Trim().ToLowerInvariant();
        }

        string SanitizeValue(object value)
        {
            if (value == null)
                return null;

            // Values should be supplied already escaped
            // But if a quote makes its way in, be a good citizen and escape it.
            return EscapeDoubleQuotes(value.ToString());
        }

        string EscapeDoubleQuotes(string value)
        {
            return value.Replace("\"", "&quot;");
        }

        public void Clear()
        {
            attributeStorage.Clear();
        }
    }
}