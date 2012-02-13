using System.Collections.Generic;
#if NET35
using Iesi.Collections.Generic;
#endif

namespace Cassette
{

#if NET40
    // Redirect for 4.0 to native
    public class HashedSet<T> : HashSet<T>
    {
        public HashedSet() : base() {}

        public HashedSet(IEnumerable<T> initialValues)
            : base(initialValues) {}
    }
#endif

#if NET35
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
#endif

#if NET40

    // Redirect to Native implementation
    public class HashedCompareSet<T> : HashSet<T> {
        public HashedCompareSet(IEqualityComparer<T> comparer)
            : base(comparer) {}

        public HashedCompareSet(ICollection<T> initialValues, IEqualityComparer<T> comparer)
            : base(initialValues, comparer) {}

        public HashedCompareSet(ICollection<T> initialValues)
            : base(initialValues) {}
    }

#endif
}
