using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iesi.Collections;
using Iesi.Collections.Generic;

namespace Cassette.Utilities
{
    public class HashedCompareSet<T> : DictionarySet<T>
    {
        public HashedCompareSet()
        {
            InternalDictionary = new Dictionary<T, object>();
        }

        public HashedCompareSet(IEqualityComparer<T> comparer)
        {
            InternalDictionary = new Dictionary<T, object>(comparer);
        }

        public HashedCompareSet(ICollection<T> initialValues)
            : this()
        {
            this.AddAll(initialValues);
        }

        public HashedCompareSet(ICollection<T> initialValues, IEqualityComparer<T> comparer)
            : this(comparer)
        {
            this.AddAll(initialValues);
        }
    }
}
