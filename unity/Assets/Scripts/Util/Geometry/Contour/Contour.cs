using System.Collections.Generic;
using System.Linq;

namespace Util.Geometry.Contour
{
    public class Contour
    {
        /// <summary>
        /// Set of vertices conforming to the requirements given in ContourPolygon.
        /// </summary>
        public List<Vector2D> Vertices { get; private set; }

        /// <summary>
        /// Holes of the contour. They are stored as the indexes of the holes in a PolygonContour class.
        /// </summary>
        public List<int> Holes { get; private set; }

        /// <summary>
        /// Whether this contour is an external contour, i.e. it is not a hole.
        /// </summary>
        public bool External { get; set; }

        public int VertexCount
        {
            get { return Vertices.Count; }
        }

        public Contour(bool external = true)
        {
            Vertices = new List<Vector2D>();
            Holes = new List<int>();
            External = external;
        }

        public Contour(IEnumerable<Vector2D> points, bool external = true)
        {
            Vertices = new List<Vector2D>(points);
            Holes = new List<int>();
            External = external;
        }

        public Contour(IEnumerable<Vector2D> vertices, IEnumerable<int> holes, bool external = true)
        {
            Vertices = new List<Vector2D>(vertices);
            Holes = new List<int>(holes);
            External = external;
        }

        public void AddVertex(Vector2D p)
        {
            Vertices.Add(p);
        }

        public void ChangeOrientation()
        {
            Vertices.Reverse();
        }

        public void AddHole(int contourId)
        {
            Holes.Add(contourId);
        }

        public void ClearHoles()
        {
            Holes.Clear();
        }

        public float Area
        {
            get
            {
                double area = 0;
                for (int i = 0; i < VertexCount - 1; i++)
                {
                    area += Vertices[i].x * Vertices[i + 1].y - Vertices[i + 1].x * Vertices[i].y;
                }

                area += Vertices[Vertices.Count - 1].x * Vertices[0].y - Vertices[0].x * Vertices[Vertices.Count - 1].y;
                return (float) (area / 2.0);
            }
        }

        public ICollection<LineSegment> Segments
        {
            get
            {
                return Enumerable.Range(0, VertexCount).Select(Segment)
                    .Select(v => new LineSegment(v[0].Vector2, v[1].Vector2)).ToList();
            }
        }

        public List<Vector2D> Segment(int i)
        {
            if (i == VertexCount - 1)
            {
                return new List<Vector2D>
                {
                    Vertices.Last(), Vertices.First()
                };
            }

            return new List<Vector2D>
            {
                Vertices[i], Vertices[i + 1]
            };
        }
    }
}