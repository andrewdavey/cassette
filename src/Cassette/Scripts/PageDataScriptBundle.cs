using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using JavaScriptObject = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>;

namespace Cassette.Scripts
{
    class PageDataScriptBundle : InlineScriptBundle
    {
        public PageDataScriptBundle(string globalVariable, object data)
            : this(globalVariable, CreateDictionaryOfProperties(data))
        {
        }

        public PageDataScriptBundle(string globalVariable, JavaScriptObject data)
            : base(BuildScript(globalVariable, data))
        {
        }

        static JavaScriptObject CreateDictionaryOfProperties(object data)
        {
            if (data == null) return null;

            return from propertyDescriptor in TypeDescriptor.GetProperties(data).Cast<PropertyDescriptor>()
                    let value = propertyDescriptor.GetValue(data)
                    select new KeyValuePair<string, object>(propertyDescriptor.Name, value);
        }

        static string BuildScript(string globalVariable, JavaScriptObject dictionary)
        {
            var builder = new StringBuilder();
            builder.AppendLine("(function(w){");
            builder.AppendFormat("var d=w['{0}']||(w['{0}']={{}});", globalVariable).AppendLine();
            BuildAssignments(dictionary, builder);
            builder.Append("}(window));");
            return builder.ToString();
        }

        static void BuildAssignments(JavaScriptObject dictionary, StringBuilder builder)
        {
            foreach (var pair in dictionary)
            {
                builder.AppendFormat("d.{0}={1};", pair.Key, JsonConvert.SerializeObject(pair.Value)).AppendLine();
            }
        }
    }
}