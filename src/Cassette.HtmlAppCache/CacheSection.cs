using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Cassette.HtmlAppCache
{
    public class CacheSection : Collection<string>
    {
        public CacheSection()
        {
        }

        CacheSection(IEnumerable<string> items)
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
            return "CACHE:\r\n" + string.Join("\r\n", this);
        }

        public static CacheSection Merge(CacheSection section1, CacheSection section2)
        {
            var items = section1.Concat(section2).Distinct();
            return new CacheSection(items);
        }
    }
}