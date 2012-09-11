using System;
using System.Collections.ObjectModel;

namespace Cassette.HtmlAppCache
{
    public class NetworkSection : Collection<string>
    {
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
    }
}