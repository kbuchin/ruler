namespace Util.Algorithms.DCEL.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.DCEL;
    using Util.Math;

    [TestFixture]
    public class HamSandwichTest
    {
        private readonly Rect m_rect;
        private readonly List<Vector2> m_points;
        private readonly List<Line> m_lines;
        private readonly DCEL m_dcel;

        private readonly List<Vector2> m_points1, m_points2, m_points3;
        private readonly List<Line> m_lines1, m_lines2, m_lines3, m_allLines;
        private readonly DCEL m_dcel1, m_dcel2, m_dcel3;

        public HamSandwichTest()
        {
            m_points = new List<Vector2>()
            {
                new Vector2(1, -2),
                new Vector2(2, 0),
                new Vector2(5, 5),
                new Vector2(-3, -1)
            };
            m_lines = new List<Line>()
            {
                new Line(1, 2),
                new Line(2, 0),
                new Line(5, -5),
                new Line(-3, 1)
            };
            m_rect = BoundingBoxComputer.FromLines(m_lines, 10f);
            m_dcel = new DCEL(m_lines, m_rect);

            m_points1 = new List<Vector2>() { new Vector2(-1, 1), new Vector2(2, -1) };
            m_points2 = new List<Vector2>() { new Vector2(3, 2), new Vector2(3.5f, 0) };
            m_points3 = new List<Vector2>() { new Vector2(-3, 1), new Vector2(-3.4f, -3) };

            m_lines1 = new List<Line>() { new Line(-1, -1), new Line(2, 1) };
            m_lines2 = new List<Line>() { new Line(3, -2), new Line(3.5f, 0) };
            m_lines3 = new List<Line>() { new Line(-3, -1), new Line(-3.4f, 3) };
            m_allLines = m_lines1.Concat(m_lines2.Concat(m_lines3)).ToList();

            m_rect = BoundingBoxComputer.FromLines(m_allLines, 10f);
            m_dcel1 = new DCEL(m_lines1, m_rect);
            m_dcel2 = new DCEL(m_lines2, m_rect);
            m_dcel3 = new DCEL(m_lines3, m_rect);
        }

        [Test]
        public void FindCutLinesSinglePointSetTest()
        {
            var ret = HamSandwich.FindCutLines(m_points);

            // 4 faces, but only 1 bouding face should be taken
            Assert.AreEqual(3, ret.Count);
        }

        [Test]
        public void FindCutLinesThreePointSetsTest()
        {
            var ret = HamSandwich.FindCutLines(m_points1, m_points2, m_points3);

            var expX = new FloatInterval(-1, 1);
            var expY = new FloatInterval(-2, 2);

            Assert.AreEqual(1, ret.Count);
            Assert.IsTrue(expX.ContainsEpsilon(ret[0].Slope));
            Assert.IsTrue(expY.ContainsEpsilon(ret[0].HeightAtYAxis));
        }

        [Test]
        public void FindCutlinesInDualSingleRegionTest()
        {
            var faces = HamSandwich.MiddleFaces(m_dcel, m_lines);
            var ret = HamSandwich.FindCutlinesInDual(faces);

            // 4 faces, but only 1 bouding face should be taken
            Assert.AreEqual(3, ret.Count);
        }

        [Test]
        public void FindCutlinesInDualThreeRegionsTest()
        {
            var faces1 = HamSandwich.MiddleFaces(m_dcel1, m_lines1);
            var faces2 = HamSandwich.MiddleFaces(m_dcel2, m_lines2);
            var faces3 = HamSandwich.MiddleFaces(m_dcel3, m_lines3);

            var ret = HamSandwich.FindCutlinesInDual(faces1, faces2, faces3);

            var expX = new FloatInterval(-1, 1);
            var expY = new FloatInterval(-2, 2);

            Assert.AreEqual(1, ret.Count);
            Assert.IsTrue(expX.ContainsEpsilon(ret[0].Slope));
            Assert.IsTrue(expY.ContainsEpsilon(ret[0].HeightAtYAxis));
        }

        [Test]
        public void MiddleFacesTest()
        {
            var ret = HamSandwich.MiddleFaces(m_dcel, m_lines);
            Assert.AreEqual(4, ret.Count);
        }
    }
}
