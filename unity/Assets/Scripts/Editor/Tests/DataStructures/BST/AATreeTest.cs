namespace Util.DataStructures.BST.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class AATreeTest
    {
        private readonly AATree<int> m_tree = new AATree<int>();

        private void CreateTree()
        {
            m_tree.Clear();
            m_tree.Insert(5);
            m_tree.Insert(0);
            m_tree.Insert(-999);
            m_tree.Insert(int.MaxValue);
        }

        [Test]
        public void InsertTest()
        {
            CreateTree();
            Assert.IsTrue(m_tree.Insert(4));
            Assert.IsTrue(m_tree.Insert(-1));
            Assert.IsTrue(m_tree.Insert(9999));
            Assert.IsTrue(m_tree.Insert(int.MinValue));
        }

        [Test]
        public void ContainsTest()
        {
            CreateTree();
            Assert.IsTrue(m_tree.Contains(5));
            Assert.IsTrue(m_tree.Contains(0));
            Assert.IsTrue(m_tree.Contains(-999));
            Assert.IsTrue(m_tree.Contains(int.MaxValue));
            Assert.IsFalse(m_tree.Contains(-1));
        }

        [Test]
        public void FindMinTest()
        {
            CreateTree();
            int val;
            Assert.IsTrue(m_tree.FindMin(out val));
            Assert.AreEqual(-999, val);
        }

        [Test]
        public void FindMaxTest()
        {
            CreateTree();
            int val;
            Assert.IsTrue(m_tree.FindMax(out val));
            Assert.AreEqual(int.MaxValue, val);
        }

        [Test]
        public void DeleteTest()
        {
            CreateTree();
            Assert.IsTrue(m_tree.Delete(5));
            Assert.IsTrue(m_tree.Delete(-999));
            Assert.IsFalse(m_tree.Delete(5));
            Assert.IsTrue(m_tree.Delete(0));
            Assert.IsFalse(m_tree.Delete(0));
        }

        [Test]
        public void DeleteMinTest()
        {
            CreateTree();
            Assert.AreEqual(-999, m_tree.DeleteMin());
            Assert.AreEqual(0, m_tree.DeleteMin());
            Assert.AreEqual(5, m_tree.DeleteMin());
            Assert.AreEqual(int.MaxValue, m_tree.DeleteMin());
        }

        [Test]
        public void DeleteMaxTest()
        {
            CreateTree();
            Assert.AreEqual(int.MaxValue, m_tree.DeleteMax());
            Assert.AreEqual(5, m_tree.DeleteMax());
            Assert.AreEqual(0, m_tree.DeleteMax());
            Assert.AreEqual(-999, m_tree.DeleteMax());
        }

        [Test]
        public void ClearTest()
        {
            CreateTree();
            m_tree.Clear();
            Assert.AreEqual(0, m_tree.Count);
        }

        [Test]
        public void FindNodesTest()
        {
            CreateTree();
            m_tree.Insert(5);
            Assert.AreEqual(5, m_tree.Count);
            Assert.AreEqual(2, m_tree.FindNodes(5).Count);
            Assert.AreEqual(1, m_tree.FindNodes(-999).Count);
            Assert.AreEqual(0, m_tree.FindNodes(-1).Count);
        }

        [Test]
        public void FindNextBiggestTest()
        {
            CreateTree();
            int val;
            Assert.IsTrue(m_tree.FindNextBiggest(0, out val));
            Assert.AreEqual(5, val);
            Assert.IsTrue(m_tree.FindNextBiggest(-999, out val));
            Assert.AreEqual(0, val);
            Assert.IsTrue(m_tree.FindNextBiggest(10, out val));
            Assert.AreEqual(int.MaxValue, val);
            Assert.IsFalse(m_tree.FindNextBiggest(int.MaxValue, out val));
        }

        [Test]
        public void FindNextSmallestTest()
        {
            CreateTree();
            int val;
            Assert.IsTrue(m_tree.FindNextSmallest(int.MaxValue, out val));
            Assert.AreEqual(5, val);
            Assert.IsTrue(m_tree.FindNextSmallest(5, out val));
            Assert.AreEqual(0, val);
            Assert.IsTrue(m_tree.FindNextSmallest(-5, out val));
            Assert.AreEqual(-999, val);
            Assert.IsFalse(m_tree.FindNextSmallest(-999, out val));
        }

        [Test]
        public void ComputeSize()
        {
            CreateTree();
            Assert.AreEqual(m_tree.Count, m_tree.ComputeSize(m_tree.Root));
            Assert.AreEqual(4, m_tree.Count);
        }

        [Test]
        public void VerifyTreeTest()
        {
            CreateTree();
            Assert.IsTrue(m_tree.VerifyBST(-999, int.MaxValue));
            Assert.IsTrue(m_tree.VerifyOrder());
            Assert.IsTrue(m_tree.VerifyLevels());
        }
    }
}
