using System.Collections.Generic;
using UnityEngine;

namespace Algo.Polygons
{
    interface ISimplePolygon
    {

        float Area();
        List<LineSegment> Segments();
        bool isConvex();
        bool Contains(Vector2 a_pos);



    }
}
