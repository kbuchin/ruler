namespace Util.Algorithms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.DataStructures.BST;
    using Util.Geometry;

    /// <summary>
    /// Class containing sweep line functions for abstract items and events.
    /// User should initialize with a given status item T and implement their own SweepEvent.
    /// 
    /// Before calling the sweeping function, one needs to initialize the event queue 
    /// and (optional) the status tree.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SweepLine<T>
        where T : IComparable<T>, IEquatable<T>
    {
        public static Line Line { get; private set; }

        // status tree used when sweeping
        private readonly IBST<T> Status = new AATree<T>();
        private readonly IBST<ISweepEvent<T>> Events = new AATree<ISweepEvent<T>>();

        public SweepLine()
        { }

        public SweepLine(Line initLine)
        {
            Line = initLine;
        }

        /// <summary>
        /// Delegate to be implemented for handling sweep events.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="status"></param>
        /// <param name="ev"></param>
        public delegate void HandleEvent(IBST<ISweepEvent<T>> events, IBST<T> status, ISweepEvent<T> ev);

        /// <summary>
        /// Initialize the status tree with the given items.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public IBST<T> InitializeStatus(IEnumerable<T> items)
        {
            Status.Clear();

            foreach (var item in items)
            {
                Status.Insert(item);
            }

            return Status;
        }

        /// <summary>
        /// Initialize the event queue with the given items.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public IBST<ISweepEvent<T>> InitializeEvents(IEnumerable<ISweepEvent<T>> items)
        {
            Events.Clear();

            foreach (var item in items)
            {
                Events.Insert(item);
            }

            return Events;
        }

        /// <summary>
        /// Performs a vertical sweep, calling the given event handler delegate.
        /// </summary>
        /// <param name="eventHandler"></param>
        public void VerticalSweep(HandleEvent eventHandler)
        {
            ISweepEvent<T> ev;
            while (Events.FindMin(out ev))
            {
                Events.DeleteMin();

                Line = new Line(ev.Pos, ev.Pos + new Vector2(1f, 0f));

                eventHandler(Events, Status, ev);
            }
        }

        /// <summary>
        /// Performs a radial sweep from the given point, using the event handler delegate.
        /// </summary>
        /// <param name="a_pos"></param>
        /// <param name="eventHandler"></param>
        public void RadialSweep(Vector2 a_pos, HandleEvent eventHandler)
        {
            ISweepEvent<T> ev;
            while(Events.FindMin(out ev))
            {
                Events.DeleteMin();

                Line = new Line(a_pos, ev.Pos);

                eventHandler(Events, Status, ev);
            }
        }
    }

    /// <summary>
    /// Interface of a sweep event, should be implemented when using the sweep line.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISweepEvent<T> : IComparable<ISweepEvent<T>>, IEquatable<ISweepEvent<T>>
        where T : IComparable<T>, IEquatable<T>
    {
        /// <summary>
        /// Position of the event
        /// </summary>
        Vector2 Pos { get; }

        /// <summary>
        /// Corresponding item for the event
        /// </summary>
        T StatusItem { get; }

        // whether the event is starting or ending
        bool IsStart { get; }
        bool IsEnd { get; }
    }

}