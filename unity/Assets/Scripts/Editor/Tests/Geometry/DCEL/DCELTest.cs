namespace Util.Geometry.DCEL.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [TestFixture]
    public class DCELTest
    {
        [Test]
        public void EmptyDCELTest()
        {
            var dcel = new DCEL();

            Assert.Zero(dcel.VertexCount);
            Assert.Zero(dcel.EdgeCount);
            Assert.AreEqual(1, dcel.FaceCount);
            Assert.Zero(dcel.InnerFaces.Count());
            Assert.Zero(dcel.OuterFace.InnerComponents.Count());
        }

        [Test]
        public void DCELFromBboxTest()
        {
            var rect = new Rect(-1, -1, 2, 2);
            var dcel = new DCEL(rect);

            Assert.AreEqual(4, dcel.VertexCount);
            Assert.AreEqual(4, dcel.EdgeCount);
            Assert.AreEqual(2, dcel.FaceCount);
            Assert.AreEqual(1, dcel.OuterFace.InnerComponents.Count());
        }

        [Test]
        public void DCELFromSegmentsTest()
        {
            var rect = new Rect(-1, -1, 2, 2);
            var segments = new List<LineSegment>()
            {
                new LineSegment(new Vector2(-1, 0), new Vector2(1, 0)),
                new LineSegment(new Vector2(0, -1), new Vector2(0, 1))
            };
            var dcel = new DCEL(segments, rect);

            Assert.AreEqual(9, dcel.VertexCount);
            Assert.AreEqual(12, dcel.EdgeCount);
            Assert.AreEqual(5, dcel.FaceCount);
            Assert.AreEqual(1, dcel.OuterFace.InnerComponents.Count());
        }

        [Test]
        public void DCELFromLinesTest()
        {
            var rect = new Rect(new Vector2(-1, -1), new Vector2(2, 2));
            var lines = new List<Line>()
            {
                new Line(new Vector2(-1, 0), new Vector2(1, 0)),
                new Line(new Vector2(0, -1), new Vector2(0, 1))
            };
            var dcel = new DCEL(lines, rect);

            Assert.AreEqual(9, dcel.VertexCount);
            Assert.AreEqual(12, dcel.EdgeCount);
            Assert.AreEqual(5, dcel.FaceCount);
            Assert.AreEqual(1, dcel.OuterFace.InnerComponents.Count());
        }

        [Test]
        public void AddVertexTest()
        {
            var dcel = new DCEL();
            var v1 = dcel.AddVertex(new DCELVertex(0, 0));
            var v2 = dcel.AddVertex(new Vector2(2, 2));
            Assert.AreEqual(2, dcel.VertexCount);

            // vertex on edge test
            var e = dcel.AddEdge(v1, v2);
            Assert.AreEqual(e, v1.Leaving);
            Assert.AreEqual(e.Twin, v2.Leaving);
            dcel.AddVertex(new Vector2(1, 1));
            Assert.AreEqual(3, dcel.VertexCount);
            Assert.AreEqual(2, dcel.EdgeCount);
        }

        [Test]
        public void AddVertexInEdgeTest()
        {
            var dcel = new DCEL();
            var v1 = dcel.AddVertex(new DCELVertex(0, 0));
            var v2 = dcel.AddVertex(new Vector2(2, 2));
            var e = dcel.AddEdge(v1, v2);
            var v3 = dcel.AddVertexInEdge(e, new Vector2(1.5f, 1.5f));
            Assert.AreEqual(3, dcel.VertexCount);
            Assert.AreEqual(2, dcel.EdgeCount);

            var e2 = new HalfEdge(v3, v1);
            Assert.Throws<GeomException>(() => dcel.AddVertexInEdge(e2, v3.Pos));
            Assert.Throws<GeomException>(() => dcel.AddVertexInEdge(e, new Vector2(-1, -1)));
        }

        [Test]
        public void AddEdgeTest()
        {
            var dcel = new DCEL();
            var v1 = dcel.AddVertex(new DCELVertex(0, 0));
            var v2 = dcel.AddVertex(new Vector2(2, 2));
            dcel.AddEdge(v1, v2);
            Assert.AreEqual(1, dcel.EdgeCount);
            dcel.AddEdge(new Vector2(0, 2), new Vector2(2, 0));
            Assert.AreEqual(4, dcel.EdgeCount);

            Assert.Throws<GeomException>(() => dcel.AddEdge(new DCELVertex(-1, -1), v1));
        }

        [Test]
        public void InnerFacesTest()
        {
            var dcel = new DCEL();

            // create large triangle
            var v1 = dcel.AddVertex(new DCELVertex(0, 0));
            var v2 = dcel.AddVertex(new DCELVertex(10, 10));
            var v3 = dcel.AddVertex(new DCELVertex(20, 0));
            dcel.AddEdge(v1, v2);
            dcel.AddEdge(v2, v3);
            dcel.AddEdge(v3, v1);


            // create smaller inner triangle
            var v4 = dcel.AddVertex(new DCELVertex(9, 2));
            var v5 = dcel.AddVertex(new DCELVertex(10, 3));
            var v6 = dcel.AddVertex(new DCELVertex(11, 2));
            dcel.AddEdge(v4, v5);
            dcel.AddEdge(v5, v6);
            dcel.AddEdge(v6, v4);

            Assert.AreEqual(6, dcel.VertexCount);
            Assert.AreEqual(6, dcel.EdgeCount);
            Assert.AreEqual(3, dcel.FaceCount);
            Assert.AreEqual(1, dcel.OuterFace.InnerComponents.Count);
            Assert.AreEqual(1, dcel.InnerFaces.FirstOrDefault().InnerComponents.Count);

            // create outside triangle
            // create smaller inner triangle
            var v7 = dcel.AddVertex(new DCELVertex(-1, -1));
            var v8 = dcel.AddVertex(new DCELVertex(-2, -2));
            var v9 = dcel.AddVertex(new DCELVertex(-3, -1));
            dcel.AddEdge(v7, v8);
            dcel.AddEdge(v8, v9);
            dcel.AddEdge(v9, v7);

            Assert.AreEqual(9, dcel.VertexCount);
            Assert.AreEqual(9, dcel.EdgeCount);
            Assert.AreEqual(4, dcel.FaceCount);
            Assert.AreEqual(2, dcel.OuterFace.InnerComponents.Count);
        }


        [Test]
        public void FindVertexTest()
        {
            var dcel = new DCEL();
            var v1 = dcel.AddVertex(new DCELVertex(1, -1));
            DCELVertex v2;
            Assert.IsTrue(dcel.FindVertex(new Vector2(1, -1), out v2));
            Assert.AreEqual(v1, v2);
            Assert.IsFalse(dcel.FindVertex(new Vector2(0, 0), out v2));
            Assert.IsNull(v2);
        }

        [Test]
        public void AdjacentEdgesTest()
        {
            var dcel = new DCEL();
            var v1 = dcel.AddVertex(new DCELVertex(0, 0));
            var v2 = dcel.AddVertex(new DCELVertex(10, 10));
            var v3 = dcel.AddVertex(new DCELVertex(20, 0));
            dcel.AddEdge(v1, v2);
            dcel.AddEdge(v2, v3);

            Assert.AreEqual(2, dcel.AdjacentEdges(v1).Count);
            Assert.AreEqual(4, dcel.AdjacentEdges(v2).Count);
            Assert.AreEqual(2, dcel.AdjacentEdges(v3).Count);
            Assert.AreEqual(1, dcel.OutgoingEdges(v1).Count);
            Assert.AreEqual(2, dcel.OutgoingEdges(v2).Count);
            Assert.AreEqual(1, dcel.OutgoingEdges(v3).Count);
        }

        [Test]
        public void GetContainingFaceTest()
        {
            var dcel = new DCEL();
            var v1 = dcel.AddVertex(new DCELVertex(0, 0));
            var v2 = dcel.AddVertex(new DCELVertex(10, 10));
            var v3 = dcel.AddVertex(new DCELVertex(20, 0));
            dcel.AddEdge(v1, v2);
            dcel.AddEdge(v2, v3);
            dcel.AddEdge(v3, v1);
            var expFace = dcel.InnerFaces.FirstOrDefault();
            Assert.AreEqual(expFace, dcel.GetContainingFace(new Vector2(10, 5)));
            Assert.AreEqual(dcel.OuterFace, dcel.GetContainingFace(new Vector2(-1, 0)));
        }

        [Test]
        public void CycleTest()
        {
            var dcel = new DCEL();
            var v1 = dcel.AddVertex(new DCELVertex(0, 0));
            var v2 = dcel.AddVertex(new DCELVertex(10, 10));
            var v3 = dcel.AddVertex(new DCELVertex(20, 0));
            var e1 = dcel.AddEdge(v1, v2);
            var e2 = dcel.AddEdge(v2, v3);
            var e3 = dcel.AddEdge(v3, v1);
            Assert.Contains(e1, DCEL.Cycle(e1));
            Assert.Contains(e2, DCEL.Cycle(e1));
            Assert.Contains(e3, DCEL.Cycle(e1));
            Assert.Contains(e1, DCEL.Cycle(e2));
            Assert.Contains(e2, DCEL.Cycle(e2));
            Assert.Contains(e3, DCEL.Cycle(e2));
            Assert.Contains(e1, DCEL.Cycle(e3));
            Assert.Contains(e2, DCEL.Cycle(e3));
            Assert.Contains(e3, DCEL.Cycle(e3));
        }
    }
}
