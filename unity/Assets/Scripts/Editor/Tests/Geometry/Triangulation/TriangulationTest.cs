namespace Util.Geometry.Triangulation.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [TestFixture]
    public class TriangulationTest
    {
        private readonly Vector2 v1, v2, v3, v4;
        private readonly TriangleEdge e1, e2, e3;
        private readonly TriangleEdge e4, e5, e6;
        private readonly Triangle t1, t2;
        private readonly Triangulation T;

        public TriangulationTest()
        {
            v1 = new Vector2(0, 0);
            v2 = new Vector2(1, 1);
            v3 = new Vector2(2, 0);
            v4 = new Vector2(1, -1);

            e1 = new TriangleEdge(v1, v2);
            e2 = new TriangleEdge(v2, v3);
            e3 = new TriangleEdge(v3, v1);
            t1 = new Triangle(e1, e2, e3);

            e4 = new TriangleEdge(v1, v3);
            e5 = new TriangleEdge(v3, v4);
            e6 = new TriangleEdge(v4, v1);
            t2 = new Triangle(e4, e5, e6);

            T = new Triangulation();
            T.AddTriangle(t1);
            T.AddTriangle(t2);
        }

        [Test]
        public void ConstructionTest()
        {
            Assert.AreEqual(2, T.Triangles.Count);
            Assert.AreEqual(6, T.Edges.Count());
            Assert.AreEqual(4, T.Vertices.Count());
            var triangles = (System.Collections.ICollection)T.Triangles;
            Assert.Contains(t1, triangles);
            Assert.Contains(t2, triangles);
        }

        [Test]
        public void AddTriangleTest()
        {
            var tr = new Triangle(v1, v2, v4);
            var T2 = new Triangulation();
            T2.AddTriangle(tr);
            Assert.AreEqual(1, T2.Triangles.Count);
        }

        [Test]
        public void AddTrianglesTest()
        {
            var T2 = new Triangulation();
            var tr = new Triangle(v1, v2, v3);
            var tr2 = new Triangle(v1, v3, v4);
            T2.AddTriangles(new List<Triangle>() { tr, tr2 });
            Assert.AreEqual(2, T2.Triangles.Count);
        }

        public void AddTriangulationTest()
        {
            var tr = new Triangle(v1, v2, v4);
            var tr2 = new Triangle();
            var T2 = new Triangulation(T.Triangles);
            var T3 = new Triangulation(new List<Triangle>() { tr, tr2 });
            T2.AddTriangulation(T3);
            Assert.AreEqual(4, T2.Triangles.Count);
        }

        [Test]
        public void RemoveTriangleTest()
        {
            var T2 = new Triangulation(T.Triangles);
            T2.RemoveTriangle(t1);
            Assert.AreEqual(1, T2.Triangles.Count);
            Assert.IsFalse(T2.Triangles.Contains(t1));
        }

        [Test]
        public void RemoveInitialTriangleTest()
        {
            var T2 = new Triangulation(v1, v2, v3);
            Assert.AreEqual(1, T2.Triangles.Count);
            T2.RemoveInitialTriangle();
            Assert.AreEqual(0, T2.Triangles.Count);
        }

        [Test]
        public void AddVertexTest()
        {
            var T2 = new Triangulation(T.Triangles);
            T2.AddVertex(new Vector2(1, 0.5f));
            Assert.AreEqual(4, T2.Triangles.Count);
            Assert.IsFalse(T2.Triangles.Contains(t1));
        }

        [Test]
        public void FindContainingTriangleTest()
        {
            var ret = T.FindContainingTriangle(new Vector2(1, 0.5f));
            Assert.AreEqual(t1, ret);
            ret = T.FindContainingTriangle(new Vector2(1, -0.5f));
            Assert.AreEqual(t2, ret);
            ret = T.FindContainingTriangle(new Vector2(-5, 0));
            Assert.IsNull(ret);
        }

        [Test]
        public void ClearTest()
        {
            var T2 = new Triangulation(T.Triangles);
            T2.Clear();
            Assert.IsEmpty(T2.Triangles);
        }

        [Test]
        public void CreateMesh()
        {
            var mesh = T.CreateMesh();
            Assert.NotNull(mesh.vertices);
            Assert.NotNull(mesh.triangles);
            Assert.IsTrue(mesh.triangles.Count() % 3 == 0);
            Assert.IsTrue(mesh.triangles.All(i => i >= 0 && i < mesh.vertices.Count()));
        }
    }
}
