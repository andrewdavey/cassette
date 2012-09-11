using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Cassette.HtmlAppCache
{
    public class NetworkSection : Collection<string>
    {
        public NetworkSection()
        {
        }

        NetworkSection(IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        protected override void InsertItem(int index, string item)
        {
            if (item == null) throw new ArgumentNullException("item", "Cannot insert a null URL string.");
            if (string.IsNullOrWhiteSpace(item)) throw new ArgumentException("Cannot insert an empty or whitespace URL string.");
            base.InsertItem(index, item);
        }

        public override string ToString()
        {
            if (Count == 0) return "";
            return "NETWORK:\r\n" + string.Join("\r\n", this);
        }

        public static NetworkSection Merge(NetworkSection section1, NetworkSection section2)
        {
            var items = section1.Concat(section2).Distinct();
            return new NetworkSection(items);
        }
    }
}