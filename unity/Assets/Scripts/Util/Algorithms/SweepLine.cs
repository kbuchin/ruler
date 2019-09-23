namespace Util.Algorithms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.DataStructures.BST;
    using Util.Geometry;

    public interface ISweepEvent<T> : IComparable<ISweepEvent<T>>, IEquatable<ISweepEvent<T>>
        where T : IComparable<T>, IEquatable<T>
    {
        Vector2 Pos { get; }

        T StatusItem { get; }

        bool IsStart { get; }

        bool IsEnd { get; }
    }

    public class SweepLine<T>
        where T : IComparable<T>, IEquatable<T>
    {
        public static Line Line { get; private set; }

        // status tree used when sweeping
        private readonly IBST<T> Status;
        private readonly IBST<ISweepEvent<T>> Events;

        public SweepLine(Line initLine)
        {
            Status = new AATree<T>();
            Events = new AATree<ISweepEvent<T>>();
            Line = initLine;
        }

        public delegate void HandleEvent(IBST<ISweepEvent<T>> events, IBST<T> status, ISweepEvent<T> ev);

        public IBST<T> InitializeStatus(IEnumerable<T> items)
        {
            Status.Clear();

            foreach (var item in items)
            {
                Status.Insert(item);
            }

            return Status;
        }

        public IBST<ISweepEvent<T>> InitializeEvents(IEnumerable<ISweepEvent<T>> items)
        {
            Events.Clear();

            foreach (var item in items)
            {
                Events.Insert(item);
            }

            return Events;
        }
        public void VerticalSweep(HandleEvent eventHandler)
        {
            ISweepEvent<T> ev;
            while (Events.FindMin(out ev))
            {
                Events.DeleteMin();

                Line = new Line(ev.Pos, ev.Pos + new Vector2(1f, 0f));

                if (ev.IsEnd)
                {
                    Debug.Log(Status.Delete(ev.StatusItem));
                }
                if (ev.IsStart)
                {
                    Debug.Log(Status.Insert(ev.StatusItem));
                }
                    

                eventHandler(Events, Status, ev);
            }
        }


        public void RadialSweep(Vector2 a_pos, HandleEvent eventHandler)
        {
            ISweepEvent<T> ev;
            while(Events.FindMin(out ev))
            {
                Events.DeleteMin();

                Line = new Line(a_pos, ev.Pos);

                /*
                if (ev.IsEnd)
                {
                    Debug.Log("remove:" + ev.StatusItem + " " + Status.Delete(ev.StatusItem));
                }
                if (ev.IsStart)
                {
                    Debug.Log("insert: " + ev.StatusItem + " " + Status.Insert(ev.StatusItem));
                }
                */

                eventHandler(Events, Status, ev);
            }
        }
    }
}