using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using JavaScriptObject = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>;

namespace Cassette.Scripts
{
    class PageDataScriptBundle : InlineScriptBundle
    {
        public PageDataScriptBundle(string globalVariable, object data, IJsonSerializer jsonSerializer)
            : this(globalVariable, CreateDictionaryOfProperties(data), jsonSerializer)
        {
        }

        public PageDataScriptBundle(string globalVariable, JavaScriptObject data, IJsonSerializer jsonSerializer)
            : base(BuildScript(globalVariable, data, jsonSerializer))
        {
        }

        static JavaScriptObject CreateDictionaryOfProperties(object data)
        {
            if (data == null) return null;

            return from propertyDescriptor in TypeDescriptor.GetProperties(data).Cast<PropertyDescriptor>()
                    let value = propertyDescriptor.GetValue(data)
                    select new KeyValuePair<string, object>(propertyDescriptor.Name, value);
        }

        static string BuildScript(string globalVariable, JavaScriptObject dictionary, IJsonSerializer jsonSerializer)
        {
            var builder = new StringBuilder();
            builder.AppendLine("(function(w){");
            builder.AppendFormat("var d=w['{0}']||(w['{0}']={{}});", globalVariable).AppendLine();
            BuildAssignments(dictionary, builder, jsonSerializer);
            builder.Append("}(window));");
            return builder.ToString();
        }

        static void BuildAssignments(JavaScriptObject dictionary, StringBuilder builder, IJsonSerializer jsonSerializer)
        {
            foreach (var pair in dictionary)
            {
                var value = jsonSerializer.Serialize(pair.Value);
                builder.AppendFormat("d.{0}={1};", pair.Key, value).AppendLine();
            }
        }
    }
}