using System;
using System.Collections.Generic;

namespace Algo.Graph
{
    internal class EdgeByMinLengthComparer : IComparer<Edge>
    {
        public int Compare(Edge x, Edge y)
        {
            return x.Length.CompareTo(y.Length);
        }
    }
}