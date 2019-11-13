namespace Util.Geometry.DCEL
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry.Polygon;

    /// <summary>
    /// Face of a DCEL structure, where edges define a polygon stored in clockwise order.
    /// 
    /// Stores a pointer to a single halfedge of the outer boundary, unless it is the outer face.
    /// Stores a single pointer to a halfedge for each inner components inside the face.
    /// 
    /// Inner components are assumed to disconnected from one another and the outer face
    /// </summary>
    public class Face
    {
        /// <summary>
        /// Points to a single half edge of the outer boundary.
        /// Used for easy iteration through the outer cycle.
        /// </summary>
        public HalfEdge OuterComponent { get; set; }

        /// <summary>
        /// Collection of half edges, one for each connected component inside the face.
        /// </summary>
        public List<HalfEdge> InnerComponents { get; private set; }

        /// <summary>
        /// Whether this face is the outer face.
        /// </summary>
        public bool IsOuter { get; set; }

        public Face(HalfEdge a_outerComponent)
        {
            OuterComponent = a_outerComponent;
            InnerComponents = new List<HalfEdge>();
            IsOuter = false;
        }

        public Face(HalfEdge a_outerComponent, List<HalfEdge> a_innerComponents)
        {
            OuterComponent = a_outerComponent;
            InnerComponents = a_innerComponents;
            IsOuter = false;
        }

        /// <summary>
        /// A list of the vertices occuring in the outer component of the face.
        /// Empty if outer face.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DCELVertex> OuterVertices
        {
            get { return OuterHalfEdges.Select(e => e.From); }
        }

        /// <summary>
        /// A list of the halfedges occuring in the outer component of the face.
        /// Empty if outer face.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HalfEdge> OuterHalfEdges
        {
            get
            {
                if (OuterComponent == null) return new List<HalfEdge>();

                return DCEL.Cycle(OuterComponent);
            }
        }

        /// <summary>
        /// Points belonging to the outer boundary.
        /// Empty if outer face.
        /// </summary>
        public IEnumerable<Vector2> OuterPoints
        {
            get { return OuterVertices.Select(v => v.Pos); }
        }

        /// <summary>
        /// All halfedges contained inside the face.
        /// Concatenates the half edges of all inner components.
        /// </summary>
        public IEnumerable<HalfEdge> InnerHalfEdges
        {
            get { return InnerComponents.SelectMany(e => DCEL.Cycle(e)); }
        }

        /// <summary>
        /// Returns a the Polygon given by the endpoints of the edges of the face.
        /// Not defined for outer face.
        /// </summary>
        /// <returns></returns>
        public Polygon2DWithHoles Polygon
        {
            get
            {
                return new Polygon2DWithHoles(PolygonWithoutHoles, InnerPolygons.Select(f => f.Outside));
            }
        }

        /// <summary>
        /// Stores outer polygon without taking into account inner holes.
        /// </summary>
        public Polygon2D PolygonWithoutHoles
        {
            get
            {
                if (IsOuter) throw new GeomException("Outer face does not have a well-defined polygon");
                return new Polygon2D(OuterPoints);
            }
        }

        /// <summary>
        /// Returns collection of 
        /// </summary>
        public IEnumerable<Polygon2DWithHoles> InnerPolygons
        {
            get
            {
                return InnerComponents
                    .Where(e => e.Twin.Face != this)    // do not select dangling edges
                    .Select(f => f.Twin.Face.Polygon);
            }
        }

        public float Area { get { return Polygon.Area; } }

        /// <summary>
        /// Returns whether the point is contained inside the face.
        /// </summary>
        /// <remarks>
        /// This assumes the face is convex
        /// </remarks>
        /// <param name="a_pos"></param>
        /// <returns></returns>
        public bool Contains(Vector2 a_pos)
        {
            if (IsOuter)
            {
                // check whether point is contained inside the outer polygon of one of the innercomponents

                return !InnerComponents.Exists(e => e.Twin.Face.PolygonWithoutHoles.ContainsInside(a_pos));
            }

            return Polygon.ContainsInside(a_pos);
        }

        /// <summary>
        /// Computes bounding rectangle around the face
        /// </summary>
        /// <param name="margin"></param>
        /// <returns></returns>
        public Rect BoundingBox(float margin = 0f)
        {
            if (IsOuter) throw new GeomException("Bounding box is ill-defined for outer face");

            return BoundingBoxComputer.FromPoints(OuterVertices.Select(x => x.Pos), margin);
        }

        public override string ToString()
        {
            if (IsOuter)
            {
                // print outer face differently
                var result = "Outer face: ";
                foreach (var comp in InnerComponents)
                {
                    result += "\nComponent: ";
                    foreach (var halfEdge in DCEL.Cycle(comp))
                    {
                        result += halfEdge.From + ", ";
                    }
                }
                return result;
            }
            else
            {
                // print all halfedges of outer face
                var result = "Face: ";
                foreach (var halfEdge in OuterHalfEdges)
                {
                    result += halfEdge.From + ", ";
                }
                foreach (var comp in InnerComponents)
                {
                    result += "\nComponent: ";
                    foreach (var halfEdge in DCEL.Cycle(comp))
                    {
                        result += halfEdge.From + ", ";
                    }
                }
                return result;
            }
        }
    }
}