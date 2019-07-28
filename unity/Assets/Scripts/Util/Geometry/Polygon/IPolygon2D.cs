namespace Util.Geometry.Polygon
{
    using UnityEngine;
    
    interface IPolygon2D
    {
        float Area();
        bool Contains(Vector2 pos);
        bool isConvex();
        bool isSimple();
    }
}
