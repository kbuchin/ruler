namespace Util.DataStructures.Queue.Tests
{
    using NUnit.Framework;
    using System;
    using System.Linq;

    [TestFixture]
    public class BinaryHeapTest
    {
        private readonly BinaryHeap<int> m_heap = new BinaryHeap<int>();

        private void CreateHeap()
        {
            m_heap.Clear();
            m_heap.Push(5);
            m_heap.Push(0);
            m_heap.Push(-999);
            m_heap.Push(int.MaxValue);
            m_heap.TrimExcess();
        }

        [Test]
        public void ContainsTest()
        {
            CreateHeap();
            Assert.IsTrue(m_heap.Contains(5));
            Assert.IsTrue(m_heap.Contains(0));
            Assert.IsTrue(m_heap.Contains(-999));
            Assert.IsTrue(m_heap.Contains(int.MaxValue));
            Assert.IsFalse(m_heap.Contains(int.MinValue));
        }

        [Test]
        public void ClearTest()
        {
            CreateHeap();
            m_heap.Clear();
            Assert.AreEqual(0, m_heap.Count());
            Assert.IsEmpty(m_heap);
        }

        [Test]
        public void PopTest()
        {
            CreateHeap();
            Assert.AreEqual(-999, m_heap.Pop());
            Assert.AreEqual(0, m_heap.Pop());
            Assert.AreEqual(5, m_heap.Pop());
            Assert.AreEqual(int.MaxValue, m_heap.Pop());
            Assert.Throws<InvalidOperationException>(() => m_heap.Pop());
        }

        [Test]
        public void PeekTest()
        {
            CreateHeap();
            Assert.AreEqual(-999, m_heap.Peek());
            m_heap.Pop();
            Assert.AreEqual(0, m_heap.Peek());
            m_heap.Pop();
            Assert.AreEqual(5, m_heap.Peek());
            m_heap.Pop();
            Assert.AreEqual(int.MaxValue, m_heap.Peek());
            m_heap.Pop();
            Assert.Throws<InvalidOperationException>(() => m_heap.Peek());
        }

        [Test]
        public void RemoveTest()
        {
            CreateHeap();
            m_heap.Remove(5);
            m_heap.RemoveAll(-999);
            m_heap.Remove(int.MaxValue);
            Assert.AreEqual(0, m_heap.Pop());
        }

        [Test]
        public void RemoveAtTest()
        {
            CreateHeap();
            m_heap.RemoveAt(0);
            Assert.AreNotEqual(-999, m_heap.Pop());
            Assert.Throws<ArgumentOutOfRangeException>(() => m_heap.RemoveAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => m_heap.RemoveAt(6));
        }

        [Test]
        public void EnumeratorTest()
        {
            CreateHeap();
            var enumerator = m_heap.GetEnumerator();
            enumerator.MoveNext();
            Assert.AreEqual(-999, (int)enumerator.Current);
            enumerator.MoveNext();
            enumerator.MoveNext();
            enumerator.MoveNext();
            Assert.IsFalse(enumerator.MoveNext());
        }
    }
}
