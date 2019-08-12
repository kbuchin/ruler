namespace Util.DataStructures.BST
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IBST<T> where T : IComparable<T>, IEquatable<T>
    {
        bool Contains(T data);
        int Count { get; }

        bool Insert(T data);

        bool FindMax(out T out_MaxValue);
        bool FindMin(out T out_MinValue);

        bool Delete(T data);
        bool DeleteMax();
        bool DeleteMin();

        void Clear();
    }
}
