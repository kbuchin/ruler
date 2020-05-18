using System;
using System.Linq;
using Util.Algorithms.Triangulation;
using Util.Geometry.Polygon;
using Util.Geometry.Triangulation;

namespace DotsAndPolygons
{
    using System.Collections.Generic;
    using UnityEngine;

    // Face
    public class UnityDotsFace : MonoBehaviour, IDotsFace
    {
        // A half-edge of the outer cycle
        public IDotsHalfEdge OuterComponent { get; set; }

        // Get list of all outer cycle half edges
        public IEnumerable<IDotsHalfEdge> OuterComponentHalfEdges
        {
            get
            {
                var visitedHalfEdges = new List<IDotsHalfEdge>();
                IDotsHalfEdge currentHalfEdge = OuterComponent.Next;
                while (true)
                {
                    if (currentHalfEdge == OuterComponent)
                    {
                        visitedHalfEdges.Add(currentHalfEdge);
                        return visitedHalfEdges;
                    }

                    if (currentHalfEdge.Next != null)
                    {
                        visitedHalfEdges.Add(currentHalfEdge);
                        currentHalfEdge = currentHalfEdge.Next;
                    }
                    else return null;
                }
            }
        }

        // Get list of all outer cycle vertices
        public IEnumerable<IDotsVertex> OuterComponentVertices =>
            OuterComponentHalfEdges?.Select(it => it.Origin) ?? new List<IDotsVertex>();

        // List of half-edges for the inner cycles bounding the face
        public List<IDotsHalfEdge> InnerComponents { get; set; }

        // Integer representing which player this face belongs to
        public int Player { get; set; }

        public float Area { get; set; }

        public float AreaMinusInner => Area - this.GetAreaOfAllInnerComponents();

        private DotsController _mGameController;

        void Awake()
        {
            _mGameController = FindObjectOfType<DotsController>();
            Player = _mGameController.CurrentPlayer == _mGameController.Player1 ? 1 : 2;
        }

        // Constructor for this face with a reference to the outer component and optional list of inner components
        public void Constructor(
            IDotsHalfEdge outerComponent,
            List<IDotsHalfEdge> innerComponents = null,
            List<Vector2> testVertices = null)
        {
            OuterComponent = outerComponent;
            InnerComponents = innerComponents ?? new List<IDotsHalfEdge>();

            List<Vector2> vertices =
                testVertices ?? OuterComponentVertices.Select(vertex => vertex.Coordinates).ToList();

            var polygon = new Polygon2D(vertices);
            Triangulation triangulation = Triangulator.Triangulate(polygon);
            Area = Math.Abs(triangulation.Area);

            Mesh mesh = triangulation.CreateMesh();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // assign the array of colors to the Mesh.
            Material material = Player == 1 ? _mGameController.p1FaceMaterial : _mGameController.p2FaceMaterial;
            var meshRenderer = (MeshRenderer) gameObject.AddComponent(typeof(MeshRenderer));
            meshRenderer.material = material;
            meshRenderer.transform.Translate(0, 0, 3);
            ((MeshFilter) gameObject.AddComponent(typeof(MeshFilter))).mesh = mesh;
        }

        public override string ToString() =>
            $"Player: {Player}, Face: {string.Join(", ", OuterComponentVertices.Select(it => it.ToString()))}";
    }
}