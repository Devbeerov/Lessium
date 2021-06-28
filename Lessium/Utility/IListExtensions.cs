using System.Collections.Generic;

namespace Lessium.Utility
{
    public static class IListExtensions
    {
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> collection)
        {
            foreach (var obj in collection)
            {
                list.Add(obj);
            }
        }

        public static bool ContainsOtherList<T>(this IList<T> list, IList<T> other)
        {
            foreach (var item in list)
            {
                if (!other.Contains(item))
                    return false;
            }

            return true;
        }
    }
}
