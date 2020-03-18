using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JetReader.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> childFunc)
        {
            return e.SelectMany(c => childFunc(c).Flatten(childFunc)).Concat(e);
        }

        public static IEnumerable<(T Item, int Depth)> FlattenWithDepth<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> childFunc, int depth = 0)
        {
            return e.SelectMany(c => childFunc(c).FlattenWithDepth(childFunc, depth + 1)).Concat(e.Select(p => (p, depth)));
        }
    }
}
