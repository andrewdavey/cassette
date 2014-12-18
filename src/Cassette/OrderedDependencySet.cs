using System.Collections.Generic;
using Cassette.Utilities;

#if NET35
using Iesi.Collections.Generic;
#endif

namespace Cassette
{
    public class OrderedDependencySet<T> : IOrderedDependencyReceiver<T>, IEnumerable<T>
    {
        private readonly HashedSet<T> membership = new HashedSet<T>();
        private readonly LinkedList<T> order = new LinkedList<T>();

        public bool Add(T item, out IOrderedDependencyReceiver<T> receiver)
        {
            receiver = null;
            if (membership.Contains(item)) return false;

            var itemNode = order.AddLast(item);
            membership.Add(item);
            receiver = new DependencyReceiver(this, itemNode);
            return true;
        }

        private bool AddDependency(LinkedListNode<T> ownerNode, T dependency, out IOrderedDependencyReceiver<T> receiver)
        {
            receiver = null;
            if (membership.Contains(dependency)) return false;

            var itemNode = order.AddBefore(ownerNode, dependency);
            membership.Add(dependency);
            receiver = new DependencyReceiver(this, itemNode);
            return true;
        }

        public bool Contains(T item)
        {
            return membership.Contains(item);
        }

        public class DependencyReceiver : IOrderedDependencyReceiver<T>
        {
            readonly OrderedDependencySet<T> set;
            readonly LinkedListNode<T> ownerNode;

            public DependencyReceiver(OrderedDependencySet<T> set, LinkedListNode<T> ownerNode)
            {
                this.set = set;
                this.ownerNode = ownerNode;
            }

            public bool Add(T item, out IOrderedDependencyReceiver<T> receiver)
            {
                return set.AddDependency(ownerNode, item, out receiver);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return order.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}