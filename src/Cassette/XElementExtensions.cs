using System;
using System.Xml.Linq;

namespace Cassette
{
    static class XElementExtensions
    {
        public static string AttributeValueOrNull(this XElement element, XName attributeName)
        {
            var attribute = element.Attribute(attributeName);
            return attribute == null ? null : attribute.Value;
        }

        public static string AttributeOrThrow(this XElement element, XName attributeName, Func<Exception> exception)
        {
            var attribute = element.Attribute(attributeName);
            if (attribute == null)
            {
                throw exception();
            }
            else
            {
                return attribute.Value;
            }
        }
    }
}