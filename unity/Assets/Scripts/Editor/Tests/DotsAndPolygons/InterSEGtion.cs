using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Util.Geometry;

namespace DotsAndPolygons.Tests
{
    using static HelperFunctions;

    public class InterSEGtion
    {
        [Test]
        public void TestInterSEGtion1()
        {
            Assert.IsTrue(
                InterSEGting(
                    new Vector2(0, 0), new Vector2(1, 1),
                    new Vector2(1, 0), new Vector2(0, 1)
                )
            );
        }

        [Test]
        public void TestInterSEGtion2()
        {
            Assert.IsFalse(
                InterSEGting(
                    new Vector2(0, 0), new Vector2(0, 1),
                    new Vector2(1, 0), new Vector2(1, 1)
                )
            );
        }

        [Test]
        public void TestInterSEGtion3()
        {
            Assert.IsFalse(
                InterSEGting(
                    new Vector2(0, 0), new Vector2(0, 1),
                    new Vector2(0, 2), new Vector2(0, 3)
                )
            );
        }

        // (a, b) (b, c)
        [Test]
        public void TestInterSEGtion4()
        {
            Assert.IsFalse(
                InterSEGting(
                    new Vector2(0, 0), new Vector2(0, 1),
                    new Vector2(0, 1), new Vector2(0, 3)
                )
            );
        }

        // (a, b) (c, b)
        [Test]
        public void TestInterSEGtion5()
        {
            Assert.IsFalse(
                InterSEGting(
                    new Vector2(0, 0), new Vector2(0, 1),
                    new Vector2(0, 3), new Vector2(0, 1)
                )
            );
        }

        // (a, b) (a, c)
        [Test]
        public void TestInterSEGtion6()
        {
            Assert.IsFalse(
                InterSEGting(
                    new Vector2(1.6f, 2.4f), new Vector2(0f, 2.1f),
                    new Vector2(1.6f, 2.4f), new Vector2(2.3f, 1.3f)
                )
            );
        }

        // (a, b) (c, a)
        [Test]
        public void TestInterSEGtion7()
        {
            Assert.IsFalse(
                InterSEGting(
                    new Vector2(1.6f, 2.4f), new Vector2(0f, 2.1f),
                    new Vector2(2.3f, 1.3f), new Vector2(1.6f, 2.4f)
                )
            );
        }

        [Test]
        public void TestInterSEGtion8()
        {
            Assert.IsFalse(
                InterSEGting(
                    new Vector2(-1.1f, 2.8f), new Vector2(-.1f, .6f),
                    new Vector2(1.2f, 1.3f), new Vector2(-.1f, .6f)
                )
            );
        }

        [Test]
        public void TestInterSEGtion9()
        {
            Assert.IsFalse(
                InterSEGting(
                    new Vector2(1.2f, 1.3f), new Vector2(-.1f, .6f),
                    new Vector2(-1.1f, 2.8f), new Vector2(-.1f, .6f)
                )
            );
        }

        [Test]
        public void TestInterSEGtion10()
        {
            Assert.IsFalse(
                InterSEGting(
                    new Vector2(-3.7f, 2.4f), new Vector2(.3f, -.8f),
                    new Vector2(-.1f, .4f), new Vector2(.3f, -.8f)
                )
            );
        }

        [Test]
        public void TestOnSeg1()
        {
            Assert.IsTrue(OnSeg(
                    new Vector2(0, 1),
                    new LineSegment(new Vector2(0, 0), new Vector2(0, 2))
                )
            );
        }

        [Test]
        public void TestOnSeg2()
        {
            Assert.IsFalse(OnSeg(
                    new Vector2(0, 1),
                    new LineSegment(new Vector2(0, 0), new Vector2(0, 1))
                )
            );
        }

        [Test]
        public void TestOnSeg3()
        {
            Assert.IsFalse(OnSeg(
                    new Vector2(0, 1),
                    new LineSegment(new Vector2(0, 1), new Vector2(0, 2))
                )
            );
        }
    }
}