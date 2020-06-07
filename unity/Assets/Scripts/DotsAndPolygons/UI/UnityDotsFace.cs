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
    public class UnityDotsFace : MonoBehaviour
    {
        public DotsFace DotsFace { get; set; }

        private DotsController _mGameController;

        void Awake()
        {
            _mGameController = FindObjectOfType<DotsController>();
            DotsFace = new DotsFace();
            DotsFace.Player = _mGameController.CurrentPlayer == _mGameController.Player1 ? 1 : 2;
        }

        // Constructor for this face with a reference to the outer component and optional list of inner components
        public void Constructor(DotsFace dotsFace)
        {
            this.DotsFace = dotsFace;

            List<Vector2> vertices =
                this.DotsFace.OuterComponentVertices.Select(vertex => vertex.Coordinates).ToList();

            var polygon = new Polygon2D(vertices);
            Triangulation triangulation = Triangulator.Triangulate(polygon);
            this.DotsFace.Area = Math.Abs(triangulation.Area);

            Mesh mesh = triangulation.CreateMesh();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // assign the array of colors to the Mesh.
            Material material = this.DotsFace.Player == 1 ? _mGameController.p1FaceMaterial : _mGameController.p2FaceMaterial;
            var meshRenderer = (MeshRenderer) gameObject.AddComponent(typeof(MeshRenderer));
            meshRenderer.material = material;
            meshRenderer.transform.Translate(0, 0, 3);
            ((MeshFilter) gameObject.AddComponent(typeof(MeshFilter))).mesh = mesh;
        }

        public override string ToString() =>
            $"Player: {this.DotsFace.Player}, Face: {string.Join(", ", this.DotsFace.OuterComponentVertices.Select(it => it.ToString()))}";

    }
}