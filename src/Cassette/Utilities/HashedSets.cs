using System.Collections.Generic;

#if NET35
using Iesi.Collections.Generic;
#endif

namespace Cassette.Utilities
{

#if !NET35
    // Redirect for 4.0 to native
    public class HashedSet<T> : HashSet<T>
    {
        public HashedSet()
        {}

        public HashedSet(IEnumerable<T> initialValues)
            : base(initialValues) {}
    }

    // Redirect to Native implementation
    public class HashedCompareSet<T> : HashSet<T> {
        public HashedCompareSet(IEnumerable<T> initialValues, IEqualityComparer<T> comparer)
            : base(initialValues, comparer) {}
    }
#else
    sealed class HashedCompareSet<T> : DictionarySet<T>
    {
        public HashedCompareSet(ICollection<T> initialValues, IEqualityComparer<T> comparer)
        {
            InternalDictionary = new Dictionary<T, object>(comparer);
            AddAll(initialValues);
        }
    }
#endif
}