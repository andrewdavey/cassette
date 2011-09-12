using System.Text;
using System.Web.Script.Serialization;
using JavaScriptObject = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>;

namespace Cassette.Scripts
{
    public class PageDataScriptModule : InlineScriptModule
    {
        public PageDataScriptModule(string globalVariable, object data)
            : this(globalVariable, CreateDictionaryOfProperties(data))
        {
        }

        public PageDataScriptModule(string globalVariable, JavaScriptObject data)
            : base(BuildScript(globalVariable, data))
        {
        }

        static JavaScriptObject CreateDictionaryOfProperties(object data)
        {
            // RouteValueDictionary has a handy ability to convert an anonymous object into dictionary.
            return new System.Web.Routing.RouteValueDictionary(data);
        }

        static string BuildScript(string globalVariable, JavaScriptObject dictionary)
        {
            var builder = new StringBuilder();
            builder.AppendLine("(function(){");
            builder.AppendFormat("var {0}=window.{0}||(window.{0}={{}});", globalVariable).AppendLine();
            BuildAssignments(globalVariable, dictionary, builder);
            builder.Append("}());");
            return builder.ToString();
        }

        static void BuildAssignments(string globalVariable, JavaScriptObject dictionary, StringBuilder builder)
        {
            var serializer = new JavaScriptSerializer();
            foreach (var pair in dictionary)
            {
                builder.AppendFormat("{0}.{1}={2};", globalVariable, pair.Key, serializer.Serialize(pair.Value)).AppendLine();
            }
        }
    }
}