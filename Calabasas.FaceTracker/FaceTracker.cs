using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Diagnostics;

namespace Calabasas
{
    internal class FaceTracker : IDisposable
    {
        private Microsoft.Kinect.Toolkit.FaceTracking.FaceTracker faceTracker;

        private bool lastFaceTrackSucceeded;

        private SkeletonTrackingState skeletonTrackingState;

        public int LastTrackedFrame { get; set; }

        public event EventHandler<System.Drawing.PointF[]> OnFaceChanged; 


        public FaceTracker()
        {}

        public void Dispose()
        {
            if (this.faceTracker != null)
            {
                this.faceTracker.Dispose();
                this.faceTracker = null;
            }
        }
        
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
                    this.faceTracker = new Microsoft.Kinect.Toolkit.FaceTracking.FaceTracker(kinectSensor);
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
                    EnumIndexableCollection<FeaturePoint, PointF> kinectFacePoints = frame.GetProjected3DShape();

                    if (this.OnFaceChanged != null)
                        this.OnFaceChanged(this, kinectFacePoints.ConvertToPointF());
                }
            }
        }
    }
}
