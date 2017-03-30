
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Algo.Polygons
{
    /// <summary>
    /// Describes a arbitrary Polygon as the union of simple polygons. We assume these polygons are non-overlapping.
    /// 
    /// </summary>
    public class VertexMultiPolygon : IPolygon
    {
        private List<VertexSimplePolygon> m_polygons;

        public VertexMultiPolygon()
        {
            m_polygons = new List<VertexSimplePolygon>();
        }

        public VertexMultiPolygon(VertexSimplePolygon a_polygon):this()
        {
            Add(a_polygon);
        }

        public ReadOnlyCollection<VertexSimplePolygon> Polygons { get { return m_polygons.AsReadOnly(); }  }

        public float Area()
        {
            var result = 0f;
            foreach(VertexSimplePolygon poly in m_polygons)
            {
                result += poly.Area();
            }
            return result;
        }

        public bool Contains(Vector2 a_pos)
        {
            foreach (VertexSimplePolygon poly in m_polygons)
            {
               if (poly.Contains(a_pos)){
                    return true;
                }
            }
            return false;
        }

        public void Add(VertexSimplePolygon a_polygon)
        {
            Debug.Assert(a_polygon != null);
            m_polygons.Add(a_polygon);
        }

        /// <summary>
        /// Cuts a_cutPoly out of this polygon
        /// </summary>
        /// <param name="a_cutPoly"></param>
        public void CutOut(VertexSimplePolygon a_cutPoly)
        {
            var result = new List<VertexSimplePolygon>();

            foreach (VertexSimplePolygon poly in m_polygons)
            {
                result.AddRange(poly.CutOut(a_cutPoly).Polygons); 
            }

            m_polygons = result;
        }
    }
}