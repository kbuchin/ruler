namespace General.UI
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry;

    /// <summary>
    /// Struct for storing a color-line tuple.
    /// </summary>
    public struct ColoredLines
    {
        public Color Color { get; private set; }
        public List<Line> Lines { get; private set; }

        public ColoredLines(Color a_color, IEnumerable<Line> a_lines)
        {
            Color = a_color;
            Lines = a_lines.ToList();
        }
    }
}
