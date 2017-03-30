using NUnit.Framework;

namespace Voronoi.Tests
{
    [TestFixture]
    [Category("Degeneracies")]
    public class VoronoiTest
    {

        [Test]
        public void TestColinearPoints()
        {
            Delaunay m_Delaunay = new Delaunay();
            m_Delaunay.Create();
            Assert.IsTrue(m_Delaunay.AddVertex(new Vertex(1, 1, Vertex.EOwnership.PLAYER1)));
            Assert.IsTrue(m_Delaunay.AddVertex(new Vertex(2, 2, Vertex.EOwnership.PLAYER1)));
            Assert.IsTrue(m_Delaunay.AddVertex(new Vertex(3, 3, Vertex.EOwnership.PLAYER1)));
            Assert.IsTrue(m_Delaunay.AddVertex(new Vertex(1, 4, Vertex.EOwnership.PLAYER1)));
            Assert.IsTrue(m_Delaunay.AddVertex(new Vertex(2, 8, Vertex.EOwnership.PLAYER1)));
            Assert.IsTrue(m_Delaunay.AddVertex(new Vertex(3, 12, Vertex.EOwnership.PLAYER1)));
        }

        [Test]
        public void TestCocircularPoints()
        {
            Delaunay m_Delaunay = new Delaunay();
            m_Delaunay.Create();
            Assert.IsTrue(m_Delaunay.AddVertex(new Vertex(1, 1, Vertex.EOwnership.PLAYER1)));
            Assert.IsTrue(m_Delaunay.AddVertex(new Vertex(1, 2, Vertex.EOwnership.PLAYER1)));
            Assert.IsTrue(m_Delaunay.AddVertex(new Vertex(2, 2, Vertex.EOwnership.PLAYER1)));
            Assert.IsTrue(m_Delaunay.AddVertex(new Vertex(2, 1, Vertex.EOwnership.PLAYER1)));
            Assert.IsTrue(m_Delaunay.AddVertex(new Vertex(1, 3, Vertex.EOwnership.PLAYER1)));
            Assert.IsTrue(m_Delaunay.AddVertex(new Vertex(2, 3, Vertex.EOwnership.PLAYER1)));
        }
    }
}
