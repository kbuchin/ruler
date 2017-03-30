using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Algo.Polygons
{
    interface IPolygon
    {

        float Area();
        bool Contains(Vector2 a_pos);

    }
}
