namespace Util.Algorithms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.DataStructures.BST;
    using Util.DataStructures.Queue;
    using Util.Geometry;

    public interface ISweepEvent<T> : IComparable
        where T : IComparable<T>, IEquatable<T>
    {
        T StatusItem { get; }

        bool IsStart { get; }

        bool IsEnd { get; }
    }

    public class SweepLine<T>
        where T : IComparable<T>, IEquatable<T>
    {
        // status tree used when sweeping
        private readonly IBST<T> Status;

        public SweepLine()
        {
            Status = new AATree<T>();
        }

        public delegate void HandleEvent(T minItem, ISweepEvent<T> ev);

        public void InitializeStatus(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Status.Insert(item);
            }
        }

        public void Sweep(List<ISweepEvent<T>> events, HandleEvent eventHandler)
        {
            events.Sort();
            
            foreach(var ev in events)
            {
                if (ev.IsEnd) Status.Delete(ev.StatusItem);
                if (ev.IsStart) Status.Insert(ev.StatusItem);

                T item;
                Status.FindMin(out item);

                eventHandler(item, ev);
            }
        }
    }
}