using System;
using System.Collections.Generic;

namespace Lessium.Utility
{
    public static class LinkedListExtensions
    {
        /// <summary>
        /// Removes excess elements beyond limit.
        /// </summary>
        /// <param name="list"></param>
        public static void RemoveExcess<T>(this LinkedList<T> list, int limit)
        {
            // Returns if no excess.

            if (limit >= list.Count) return;

            // Calculates difference between limit and actual count.

            var difference = Math.Abs(limit - list.Count);

            // Removes elements from the end.

            for (int i = 0; i < difference; i++)
            {
                list.RemoveLast();
            }
        }
    }
}
