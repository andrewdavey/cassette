using System;
using System.Collections.ObjectModel;

namespace Cassette.HtmlAppCache
{
    public class CacheSection : Collection<string>
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
            return "CACHE:\r\n" + string.Join("\r\n", this);
        }
    }
}