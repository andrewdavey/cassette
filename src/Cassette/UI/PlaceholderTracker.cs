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

using System;
using System.Collections.Generic;
using System.Text;

namespace Cassette.UI
{
    public class PlaceholderTracker : IPlaceholderTracker
    {
        readonly Dictionary<Guid, Func<string>> creationFunctions = new Dictionary<Guid, Func<string>>();

        public string InsertPlaceholder(Func<string> futureHtml)
        {
            var id = Guid.NewGuid();
            creationFunctions[id] = futureHtml;
            return Environment.NewLine + id.ToString() + Environment.NewLine;
        }

        public string ReplacePlaceholders(string html)
        {
            var builder = new StringBuilder(html);
            foreach (var item in creationFunctions)
            {
                builder.Replace(item.Key.ToString(), item.Value());
            }
            return builder.ToString();
        }
    }
}
