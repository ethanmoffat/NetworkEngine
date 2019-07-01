
using System.Collections;
using System.Collections.Generic;

namespace NetworkEngine.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
                yield return (T)enumerator.Current;
        }
    }
}