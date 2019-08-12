namespace Util.DataStructures.Queue
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class BinaryHeap<T> : IPriorityQueue<T>
    {
        private readonly IComparer<T> Comparer;
        private readonly List<T> Items = new List<T>();

        public BinaryHeap() : this(Comparer<T>.Default)
        { }

        public BinaryHeap(IComparer<T> comp)
        {
            Comparer = comp;
        }

        public BinaryHeap(IEnumerable<T> init) : this(init, Comparer<T>.Default)
        { }

        public BinaryHeap(IEnumerable<T> init, IComparer<T> comp)
        {
            Comparer = comp;
            BuildHeap(init);
        }

        /// <summary>
        /// Builds new heap from list of items
        /// </summary>
        /// <remarks>
        /// Clears any items currently in the heap
        /// </remarks>
        public void BuildHeap(IEnumerable<T> init)
        {
            
            Items.Clear();
            foreach (T item in init) Items.Add(item);

            // heapify starting from the bottom subtrees upwards
            for (int i = Items.Count / 2 + 1; i >= 0; i--) Heapify(i);
        }


        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            Items.Clear();
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the BinaryHeap,
        /// if that number is less than a threshold value.
        /// </summary>
        /// <remarks>
        /// The current threshold value is 90% (.NET 3.5), but might change in a future release.
        /// </remarks>
        public void TrimExcess()
        {
            Items.TrimExcess();
        }

        /// <summary>
        /// Inserts an item onto the heap.
        /// </summary>
        /// <param name="newItem">The item to be inserted.</param>
        public void Push(T newItem)
        {
            // add item at bottom of heap
            Items.Add(newItem);

            // move item upwards
            int i = Count - 1;
            while (i > 0 && Comparer.Compare(Items[(i - 1) / 2], newItem) > 0)
            {
                Items[i] = Items[(i - 1) / 2];
                i = (i - 1) / 2;
            }

            Items[i] = newItem;
        }

        /// <summary>
        /// Return the root item from the collection, without removing it.
        /// </summary>
        /// <returns>Returns the item at the root of the heap.</returns>
        public T Peek()
        {
            if (Items.Count == 0)
            {
                throw new InvalidOperationException("The heap is empty.");
            }
            return Items[0];
        }

        /// <summary>
        /// Removes and returns the root item from the collection.
        /// </summary>
        /// <returns>Returns the item at the root of the heap.</returns>
        public T Pop()
        {
            if (Items.Count == 0)
            {
                throw new InvalidOperationException("The heap is empty.");
            }

            // Get the first item
            T rslt = Items[0];

            // Remove head and satisfy heap 
            RemoveAt(0);

            return rslt;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns>Whether heap contains value.</returns>
        public bool Contains(T val)
        {
            return Items.Contains(val);
        }

        /// <summary>
        /// Removes first node with given value.
        /// </summary>
        /// <param name="val"></param>
        public void Remove(T val)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Comparer.Compare(Items[i], val) == 0)
                {
                    RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Remove all nodes with given value.
        /// </summary>
        /// <param name="val"></param>
        public void RemoveAll(T val)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Comparer.Compare(Items[i], val) == 0)
                {
                    RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Get a count of the number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return Items.Count; }
        }

        /// <summary>
        /// Removes item at given index.
        /// </summary>
        /// <param name="i">Index into the list <paramref name="Items"/></param>
        internal void RemoveAt(int i)
        {
            if(i >= Items.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            // move last item to current position
            Items[i] = Items[Items.Count - 1];
            Items.RemoveAt(Items.Count - 1);

            // restore heap property
            Heapify(i);
        }

        /// <summary>
        /// Changes heap top-down to enforce heap property.
        /// </summary>
        /// <param name="i">Index into the list <paramref name="Items"/>.</param>
        internal void Heapify(int i)
        {
            if (i >= Items.Count) return;

            // get children in list
            int left = 2 * (i + 1) - 1;
            int right = 2 * (i + 1);
            int largest = i;

            // find index of largest value
            if (left < Items.Count && Comparer.Compare(Items[left], Items[largest]) < 0) largest = left;
            if (right < Items.Count && Comparer.Compare(Items[right], Items[largest]) < 0) largest = right;

            if (largest != i)
            {
                // swap values
                T tmp = Items[i];
                Items[i] = Items[largest];
                Items[largest] = tmp;

                // recurse on child
                Heapify(largest);
            }
        }
    }
}