namespace Util.DataStructures.Queue
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Simple binary heap implementation of a priority queue.
    /// Can set comparer to switch between min/max or other orderings.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
        /// Get a count of the number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return Items.Count; }
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


        public void Push(T newItem)
        {
            // add item at bottom of heap
            Items.Add(newItem);

            // move item upwards
            var i = Count - 1;
            while (i > 0 && Comparer.Compare(Items[(i - 1) / 2], newItem) > 0)
            {
                Items[i] = Items[(i - 1) / 2];
                i = (i - 1) / 2;
            }

            Items[i] = newItem;
        }

        public T Peek()
        {
            if (Items.Count == 0)
            {
                throw new InvalidOperationException("The heap is empty.");
            }
            return Items[0];
        }

        public T Pop()
        {
            if (Items.Count == 0)
            {
                throw new InvalidOperationException("The heap is empty.");
            }

            // Get the first item
            var rslt = Items[0];

            // Remove head and satisfy heap 
            RemoveAt(0);

            return rslt;
        }

        public bool Contains(T val)
        {
            return Items.Contains(val);
        }

        public void Remove(T val)
        {
            var i = Items.FindIndex(item => Comparer.Compare(item, val) == 0);
            if (i != -1)
            {
                RemoveAt(i);
            }
        }

        public void RemoveAll(T val)
        {
            Items.RemoveAll(item => Comparer.Compare(item, val) == 0);
        }

        public void Clear()
        {
            Items.Clear();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        /// <summary>
        /// Removes item at given index.
        /// </summary>
        /// <param name="i">Index into the list <paramref name="Items"/></param>
        public void RemoveAt(int i)
        {
            if (i < 0 || i >= Items.Count)
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
    }
}