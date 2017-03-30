using Algo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace General
{
    class ColoredLines
    {
        private Color m_color;
        private List<Line> m_lines;

        public Color  Color{get{ return m_color; } }
        public List<Line> Lines {get{ return m_lines; } }


        internal ColoredLines(Color a_color, List<Line> a_lines)
        {
            m_color = a_color;
            m_lines = a_lines;
        }

    }
}
