namespace Util.DataStructures.BST
{
    using System;

    /// <summary>
    /// Generic interface for a binary search tree (BST).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBST<T> where T : IComparable<T>, IEquatable<T>
    {
        /// <summary>
        /// Check whether the tree contains the given data value.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        bool Contains(T data);

        /// <summary>
        /// Count the number of nodes in the tree.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Inserts a new data value into the tree.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>whether the insertion was succesful.</returns>
        bool Insert(T data);

        /// <summary>
        /// Finds the maximum value in the tree and sets it to the out variable.
        /// </summary>
        /// <param name="out_MaxValue"></param>
        /// <returns>whether the maximum was found</returns>
        bool FindMax(out T out_MaxValue);

        /// <summary>
        /// Finds the minimum value in the tree and sets it to the out variable.
        /// </summary>
        /// <param name="out_MaxValue"></param>
        /// <returns>whether the minimum was found</returns>
        bool FindMin(out T out_MinValue);

        /// <summary>
        /// Finds next smaller compared to given data value.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="out_NextSmallest"></param>
        /// <returns>whether the method was succesful.</returns>
        bool FindNextSmallest(T data, out T out_NextSmallest);

        /// <summary>
        /// Finds next bigger value compared to given data value.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="out_NextBiggest"></param>
        /// <returns>whether the method was succesful.</returns>
        bool FindNextBiggest(T data, out T out_NextBiggest);

        /// <summary>
        /// Delete the given data value from the tree.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>whether the deletion was succeful.</returns>
        bool Delete(T data);

        /// <summary>
        /// Deletes the maximum value from the tree.
        /// </summary>
        /// <returns>the given maximum value that was removed</returns>
        T DeleteMax();

        /// <summary>
        /// Deletes the maximum value from the tree.
        /// </summary>
        /// <returns>the given maximum value that was removed</returns>
        T DeleteMin();

        /// <summary>
        /// Clears the tree of all nodes.
        /// </summary>
        void Clear();
    }
}
