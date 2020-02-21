using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Util.Geometry.Contour;
using Util.Geometry.Polygon;

namespace Util.Algorithms.Polygon
{
    /// <summary>
    /// Implements the <see cref="Union"/> method by using a Martinez sweep line approach using <see cref="Martinez"/>.
    /// </summary>
    public class UnionSweepLine : IUnion
    {
        /// <inheritdoc />
        public IPolygon2D Union(ICollection<Polygon2D> polygons)
        {
            if (polygons.Count == 0)
            {
                return new MultiPolygon2D();
            }

            var builder = new StringBuilder();
            builder.AppendLine("If the union crashes, use the following to create a unit test in IUnionTests:");

            int i = 0;
            foreach (var polygon2D in polygons)
            {
                builder.AppendFormat("var polygon{0} = new Polygon2D(new List<Vector2> {1}", i++, "{");
                foreach (var vertex in polygon2D.Vertices)
                {
                    // We are using base64 to make sure we get the exact bytes for the float rather than slightly different bytes.
                    // These are mostly robustness issues, so the differences are very small.
                    builder.AppendFormat(
                        "new Vector2(BitConverter.ToSingle(Convert.FromBase64String(\"{0}\"), 0), BitConverter.ToSingle(Convert.FromBase64String(\"{1}\"), 0)), ",
                        Convert.ToBase64String(BitConverter.GetBytes(vertex.x)),
                        Convert.ToBase64String(BitConverter.GetBytes(vertex.y)));
                }

                builder.Append("});\n");
            }

            //Debug.Log(builder.ToString());

            var result = polygons.First().ToContourPolygon();

            foreach (var polygon in polygons.Skip(1))
            {
                var martinez = new Martinez(result, polygon.ToContourPolygon(), Martinez.OperationType.Union);

                result = martinez.Run();
            }

            return result;
        }
    }
}