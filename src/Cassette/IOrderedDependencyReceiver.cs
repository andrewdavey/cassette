namespace Cassette
{
    public interface IOrderedDependencyReceiver<T>
    {
        /// <summary>
        /// Adds an item to the set and provides a bookmark for inserting its dependencies.
        /// </summary>
        /// <param name="item">Item to add.</param>
        /// <param name="receiver">Subset to which the item's dependencies should be added.</param>
        /// <returns>True if the item was not already present. False if the item (and presumably its dependencies) is already present.</returns>
        bool Add(T item, out IOrderedDependencyReceiver<T> receiver);
    }
}