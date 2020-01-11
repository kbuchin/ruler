using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArtGallery
{
    /// <summary>
    ///     Class containing extension methods for
    ///     <see cref="IEnumerable{T}" />
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        ///     Changes the
        ///     <paramref name="collection" />
        ///     such that the collection starts with
        ///     <paramref name="start" />
        /// </summary>
        /// <typeparam name="T">The element type</typeparam>
        /// <param name="collection"> The collection</param>
        /// <param name="start"> The starting element in the collection</param>
        /// <returns>
        ///     <paramref name="collection" />
        ///     starting with the element indicated by
        ///     <paramref name="start" />
        /// </returns>
        public static IEnumerable<T> StartAt<T>(
            this IEnumerable<T> collection,
            T start)
        {
            var collectionList = collection.ToList();
            int vIndex = -1;
            int collectionCount = collectionList.Count;

            for (int i = 0; i < collectionCount; i++)
            {
                if (collectionList[i].Equals(start))
                {
                    vIndex = i;

                    break;
                }
            }

            if (vIndex == -1)
            {
                throw new ArgumentException(
                    "The element start does not occur in the collection");
            }

            // adjusts positions such that start at the beginning
            var result = collectionList.Skip(vIndex)
                                       .Concat(collectionList.Take(vIndex))
                ;

            return result;
        }
    }
}