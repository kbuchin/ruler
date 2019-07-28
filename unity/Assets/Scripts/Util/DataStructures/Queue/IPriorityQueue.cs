namespace Util.DataStructures.Queue
{
    using System.Collections.Generic;

    /// <summary>
    /// The IPriorityQueue interface
    /// </summary>
    public interface IPriorityQueue<T> : IEnumerable<T>
    {
        /// <summary>
        /// Enqueue a node to the priority queue.  Lower values are placed in front. Ties are broken by first-in-first-out.
        /// See implementation for how duplicates are handled.
        /// </summary>
        void Push(T val);

        /// <summary>
        /// Removes the head of the queue (node with minimum priority; ties are broken by order of insertion), and returns it.
        /// </summary>
        T Pop();

        /// <summary>
        /// Removes every node from the queue.
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns whether the given value is in the queue.
        /// </summary>
        bool Contains(T val);

        /// <summary>
        /// Removes first node with given value from the queue.  The node does not need to be the head of the queue.  
        /// </summary>
        void Remove(T val);

        /// <summary>
        /// Removes all nodes with given value from the queue.  The node does not need to be the head of the queue.  
        /// </summary>
        void RemoveAll(T val);

        /// <summary>
        /// Returns the head of the queue, without removing it (use Dequeue() for that).
        /// </summary>
        T Peek();

        /// <summary>
        /// Returns the number of nodes in the queue.
        /// </summary>
        int Count { get; }
    }
}
