using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AngelNode.Extension
{
    public static class ObservableCollectionExtensions
    {
        public static void Move<T>(this ObservableCollection<T> collection, T oldObject, int newObjectPosition)
        {
            (bool shouldMove, int newPos) = IsSamePosition(collection, oldObject, newObjectPosition);

            if (!shouldMove) return;

            var objectIndex = collection.IndexOf(oldObject);
            collection.Move(objectIndex, newPos);
        }

        public static (bool shouldMove, int newPos) IsSamePosition<T>(ObservableCollection<T> collection, T oldObject, int position)
        {
            var objectIndex = collection.IndexOf(oldObject);
            if (position > objectIndex) position--;
            return (position != objectIndex, position);
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items) collection.Add(item);
        }
    }
}
