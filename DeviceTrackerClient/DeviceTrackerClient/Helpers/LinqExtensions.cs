using System;
using System.Collections.Generic;
using System.Linq;

namespace DeviceTrackerClient.Helpers
{
    public static class LinqExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<T> Except<T>(
            this IEnumerable<T> first,
            IEnumerable<T> second,
            IEqualityComparer<T> comparer = null)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            return ExceptIterator(first, second, comparer ?? EqualityComparer<T>.Default);
        }

        private static IEnumerable<T> ExceptIterator<T>(
            IEnumerable<T> first,
            IEnumerable<T> second,
            IEqualityComparer<T> comparer)
        {
            var set = new HashSet<T>(second, comparer);
            foreach (var element in first)
            {
                if (set.Add(element))
                {
                    yield return element;
                }
            }
        }
    }
}