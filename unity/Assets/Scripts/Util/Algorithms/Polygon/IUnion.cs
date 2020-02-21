namespace Util.Algorithms.Polygon
{
    using System.Collections.Generic;
    using Util.Geometry.Polygon;

    /// <summary>
    /// This interface defines methods that can be used to calculate the union
    /// of polygons
    /// </summary>
    public interface IUnion
    {
        /// <summary>
        /// Calculates the union of <paramref name="polygons"/>
        /// </summary>
        /// <param name="polygons">
        /// The polygons to calculate the union of
        /// </param>
        /// <returns>A new polygon which is the union of the polygons defined in
        /// <paramref name="polygons"/></returns>
        IPolygon2D Union(ICollection<Polygon2D> polygons);
    }
}