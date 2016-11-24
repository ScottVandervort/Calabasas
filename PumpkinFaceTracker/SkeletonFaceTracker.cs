using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PumpkinFaceTracker
{
    public class SkeletonFaceTracker : IDisposable
    {
        private static FaceTriangle[] faceTriangles;

        private EnumIndexableCollection<FeaturePoint, PointF> facePoints;

        private FaceTracker faceTracker;

        private bool lastFaceTrackSucceeded;

        private SkeletonTrackingState skeletonTrackingState;

        public int LastTrackedFrame { get; set; }

        public void Dispose()
        {
            if (this.faceTracker != null)
            {
                this.faceTracker.Dispose();
                this.faceTracker = null;
            }
        }

        //public void DrawFaceModel(DrawingContext drawingContext)
        //{
        //    if (!this.lastFaceTrackSucceeded || this.skeletonTrackingState != SkeletonTrackingState.Tracked)
        //    {
        //        return;
        //    }

        //    var faceModelPts = new List<Point>();
        //    var faceModel = new List<FaceModelTriangle>();

        //    for (int i = 0; i < this.facePoints.Count; i++)
        //    {
        //        faceModelPts.Add(new Point(this.facePoints[i].X + 0.5f, this.facePoints[i].Y + 0.5f));
        //    }

        //    foreach (var t in faceTriangles)
        //    {
        //        var triangle = new FaceModelTriangle();
        //        triangle.P1 = faceModelPts[t.First];
        //        triangle.P2 = faceModelPts[t.Second];
        //        triangle.P3 = faceModelPts[t.Third];
        //        faceModel.Add(triangle);
        //    }

        //    var faceModelGroup = new GeometryGroup();
        //    for (int i = 0; i < faceModel.Count; i++)
        //    {
        //        var faceTriangle = new GeometryGroup();
        //        faceTriangle.Children.Add(new LineGeometry(faceModel[i].P1, faceModel[i].P2));
        //        faceTriangle.Children.Add(new LineGeometry(faceModel[i].P2, faceModel[i].P3));
        //        faceTriangle.Children.Add(new LineGeometry(faceModel[i].P3, faceModel[i].P1));
        //        faceModelGroup.Children.Add(faceTriangle);
        //    }

        //    drawingContext.DrawGeometry(Brushes.LightYellow, new Pen(Brushes.LightYellow, 1.0), faceModelGroup);
        //}

        /// <summary>
        /// Updates the face tracking information for this skeleton
        /// </summary>
        internal void OnFrameReady(KinectSensor kinectSensor, ColorImageFormat colorImageFormat, byte[] colorImage, DepthImageFormat depthImageFormat, short[] depthImage, Skeleton skeletonOfInterest)
        {
            this.skeletonTrackingState = skeletonOfInterest.TrackingState;

            if (this.skeletonTrackingState != SkeletonTrackingState.Tracked)
            {
                // nothing to do with an untracked skeleton.
                return;
            }

            if (this.faceTracker == null)
            {
                try
                {
                    this.faceTracker = new FaceTracker(kinectSensor);
                }
                catch (InvalidOperationException)
                {
                    // During some shutdown scenarios the FaceTracker
                    // is unable to be instantiated.  Catch that exception
                    // and don't track a face.
                    Debug.WriteLine("AllFramesReady - creating a new FaceTracker threw an InvalidOperationException");
                    this.faceTracker = null;
                }
            }

            if (this.faceTracker != null)
            {
                FaceTrackFrame frame = this.faceTracker.Track(
                    colorImageFormat, colorImage, depthImageFormat, depthImage, skeletonOfInterest);

                this.lastFaceTrackSucceeded = frame.TrackSuccessful;
                if (this.lastFaceTrackSucceeded)
                {
                    if (faceTriangles == null)
                    {
                        // only need to get this once.  It doesn't change.
                        faceTriangles = frame.GetTriangles();
                    }

                    this.facePoints = frame.GetProjected3DShape();

                    Console.WriteLine(facePoints[FeaturePoint.AboveChin].X + "," + facePoints[FeaturePoint.AboveChin].Y);
                }
            }
        }

        private struct FaceModelTriangle
        {
            public Point P1;
            public Point P2;
            public Point P3;
        }
    }
}
