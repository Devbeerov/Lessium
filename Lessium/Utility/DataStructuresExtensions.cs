using System.Collections.Generic;
using System.Linq;

namespace Lessium.Utility
{
    public static class DataStructuresExtensions
    {
        public static bool IsSame<T>(IEnumerable<T> set1, IEnumerable<T> set2)
        {
            // Primal checks

            if (set1 == null && set2 == null) return true; // Both null
            if (set1 == null || set2 == null) return false; // One is null

            // Converts to List to use Sort method and Count property.

            List<T> list1 = set1.ToList();
            List<T> list2 = set2.ToList();

            // Count check

            if (list1.Count != list2.Count) return false;

            // Sorting

            list1.Sort();
            list2.Sort();

            // Checks if sorted sequences are same.

            return list1.SequenceEqual(list2);
        }
    }
}
