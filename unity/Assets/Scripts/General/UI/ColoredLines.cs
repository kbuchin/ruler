namespace General.Drawing
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry;

    public struct ColoredLines
    {
        public Color Color { get; private set; }
        public ICollection<Line> Lines { get; private set; }

        internal ColoredLines(Color a_color, ICollection<Line> a_lines)
        {
            Color = a_color;
            Lines = a_lines;
        }
    }
}
