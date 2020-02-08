using System;
using System.Collections.Generic;

namespace Utils
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// A functional replacement for a <see langword="foreach"/> block.
        /// </summary>
        /// <param name="collection">The <see cref="IEnumerable{T}"/> that will be iterated over.</param>
        /// <param name="action">The <see cref="Action{T}"/> that will be run over each item in the <paramref name="collection"/>.</param>
        /// <typeparam name="T">The type of items in the <paramref name="collection"/>.</typeparam>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }
    }
}