﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Calabasas.Test
{
    [TestClass]
    public class FaceStateTest
    {
        [TestMethod]
        public void TestDefaultValues()
        {
            FaceState faceState = new FaceState();

            Assert.IsTrue(faceState.boundingBox == System.Drawing.RectangleF.Empty);


            Assert.IsTrue(faceState.IsHappy == false);
            Assert.IsTrue(faceState.IsLeftEyeClosed == false);
            Assert.IsTrue(faceState.IsRightEyeClosed == false);
            Assert.IsTrue(faceState.IsMouthMoved == false);
            Assert.IsTrue(faceState.IsMouthOpen == false);
            Assert.IsTrue(faceState.IsWearingGlasses == false);
            Assert.IsTrue(faceState.Points == null);
        }

        [TestMethod]
        public void TestGetBoundingBox()
        {
            System.Drawing.RectangleF boundingBox = FaceState.DetermineBoundingBox(new System.Drawing.PointF[]
            {
                new System.Drawing.PointF(0,0),
                new System.Drawing.PointF(0,500),
                new System.Drawing.PointF(200,0),
                new System.Drawing.PointF(100,100)
            });

            Assert.IsTrue(boundingBox.Width == 200.0F);
            Assert.IsTrue(boundingBox.Height == 500.0F);
            Assert.IsTrue(boundingBox.X == 0);
            Assert.IsTrue(boundingBox.Y == 0);
        }

        [TestMethod]
        public void TestDetermineBoundingBox_NoPoints()
        {
            System.Drawing.RectangleF boundingBox = FaceState.DetermineBoundingBox(new System.Drawing.PointF[] { });
            Assert.IsTrue(boundingBox == System.Drawing.RectangleF.Empty);

            boundingBox = FaceState.DetermineBoundingBox(null);
            Assert.IsTrue(boundingBox == System.Drawing.RectangleF.Empty);
        }

        [TestMethod]
        public void TestBoundingBox()
        {
            FaceState faceState = new FaceState();

            faceState.Points = new System.Drawing.PointF[]
              {
                new System.Drawing.PointF(20,20),
                new System.Drawing.PointF(15,15),
                new System.Drawing.PointF(0,20),
                new System.Drawing.PointF(15,40),
                new System.Drawing.PointF(10,60),
                new System.Drawing.PointF(5,40),
                new System.Drawing.PointF(10,10),
                new System.Drawing.PointF(5,15)
              };

            Assert.IsTrue(20.0F == faceState.BoundingBox.Width);
            Assert.IsTrue(50.0F == faceState.BoundingBox.Height);
            Assert.IsTrue(faceState.BoundingBox.X == 0);
            Assert.IsTrue(faceState.BoundingBox.Y == 10);
        }

        [TestMethod]
        public void TestBoundingBox_Caching()
        {
            FaceState faceState = new FaceState();

            faceState.boundingBox = new System.Drawing.RectangleF(1, 2, 3, 4);
            faceState.Points = new System.Drawing.PointF[]
              {
                new System.Drawing.PointF(20,20),
                new System.Drawing.PointF(15,15),
                new System.Drawing.PointF(0,20),
                new System.Drawing.PointF(15,40),
                new System.Drawing.PointF(10,60),
                new System.Drawing.PointF(5,40),
                new System.Drawing.PointF(10,10),
                new System.Drawing.PointF(5,15)
              };
                    
            Assert.IsTrue(System.Drawing.RectangleF.Equals(new System.Drawing.RectangleF(1, 2, 3, 4), faceState.BoundingBox));
        }
    }
}
