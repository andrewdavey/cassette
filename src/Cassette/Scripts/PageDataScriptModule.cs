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
using System.Collections.Generic;
using Cassette.Json;
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

        internal static JavaScriptObject CreateDictionaryOfProperties(object data)
        {
            // Chris Hogan : Replaced dependency on System.Web.Routing.RouteValueDictionary
            var result = new List<KeyValuePair<string,object>>();
            if(data == null)
            {
              return result;
            }

            foreach (var prop in data.GetType().GetProperties())
            {
                try
                {
                  result.Add(new KeyValuePair<string, object>(prop.Name, prop.GetValue(data, null)));
                }
                catch
                {
                }
            }

            return result;
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
