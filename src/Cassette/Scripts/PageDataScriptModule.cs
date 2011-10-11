#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System.Text;
using System.Web.Script.Serialization;
using JavaScriptObject = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>;

namespace Cassette.Scripts
{
    public class PageDataScriptBundle : InlineScriptBundle
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
